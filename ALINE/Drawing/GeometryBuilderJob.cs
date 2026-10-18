using System;
using System.Runtime.CompilerServices;
using Drawing.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drawing;

[BurstCompile(FloatMode = FloatMode.Default)]
internal struct GeometryBuilderJob : IJob
{
	public struct Vertex
	{
		public float3 position;

		public float3 uv2;

		public Color32 color;

		public float2 uv;
	}

	public struct TextVertex
	{
		public float3 position;

		public Color32 color;

		public float2 uv;
	}

	[NativeDisableUnsafePtrRestriction]
	public unsafe DrawingData.ProcessedBuilderData.MeshBuffers* buffers;

	[NativeDisableUnsafePtrRestriction]
	public unsafe SDFCharacter* characterInfo;

	public int characterInfoLength;

	public Color32 currentColor;

	public float4x4 currentMatrix;

	public CommandBuilder.LineWidthData currentLineWidthData;

	public float lineWidthMultiplier;

	private float3 minBounds;

	private float3 maxBounds;

	public float3 cameraPosition;

	public quaternion cameraRotation;

	public float2 cameraDepthToPixelSize;

	public float maxPixelError;

	public bool cameraIsOrthographic;

	private float3 lastNormalizedLineDir;

	private float lastLineWidth;

	public const float MaxCirclePixelError = 0.5f;

	public const int VerticesPerCharacter = 4;

	public const int TrianglesPerCharacter = 6;

	internal static readonly float4[] BoxVertices = new float4[8]
	{
		new float4(-1f, -1f, -1f, 1f),
		new float4(-1f, -1f, 1f, 1f),
		new float4(-1f, 1f, -1f, 1f),
		new float4(-1f, 1f, 1f, 1f),
		new float4(1f, -1f, -1f, 1f),
		new float4(1f, -1f, 1f, 1f),
		new float4(1f, 1f, -1f, 1f),
		new float4(1f, 1f, 1f, 1f)
	};

	internal static readonly int[] BoxTriangles = new int[36]
	{
		0, 1, 5, 0, 5, 4, 7, 3, 2, 7,
		2, 6, 0, 1, 3, 0, 3, 2, 4, 5,
		7, 4, 7, 6, 1, 3, 7, 1, 7, 5,
		0, 2, 6, 0, 6, 4
	};

	public const int MaxStackSize = 32;

	private unsafe static void Add<T>(UnsafeAppendBuffer* buffer, T value) where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		*(T*)(buffer->Ptr + buffer->Length) = value;
		buffer->Length += num;
	}

	private unsafe static void Reserve(UnsafeAppendBuffer* buffer, int size)
	{
		int num = buffer->Length + size;
		if (num > buffer->Capacity)
		{
			buffer->SetCapacity(math.max(num, buffer->Capacity * 2));
		}
	}

	internal static float3 PerspectiveDivide(float4 p)
	{
		return p.xyz * math.rcp(p.w);
	}

	private unsafe void AddText(ushort* text, CommandBuilder.TextData textData, Color32 color)
	{
		float3 pivot = PerspectiveDivide(math.mul(currentMatrix, new float4(textData.center, 1f)));
		AddTextInternal(text, pivot, math.mul(cameraRotation, new float3(1f, 0f, 0f)), math.mul(cameraRotation, new float3(0f, 1f, 0f)), textData.alignment, textData.sizeInPixels, sizeIsInPixels: true, textData.numCharacters, color);
	}

	private unsafe void AddText3D(ushort* text, CommandBuilder.TextData3D textData, Color32 color)
	{
		float3 pivot = PerspectiveDivide(math.mul(currentMatrix, new float4(textData.center, 1f)));
		float4x4 float4x = math.mul(currentMatrix, new float4x4(textData.rotation, float3.zero));
		AddTextInternal(text, pivot, float4x.c0.xyz, float4x.c1.xyz, textData.alignment, textData.size, sizeIsInPixels: false, textData.numCharacters, color);
	}

	private unsafe void AddTextInternal(ushort* text, float3 pivot, float3 right, float3 up, LabelAlignment alignment, float size, bool sizeIsInPixels, int numCharacters, Color32 color)
	{
		float num = math.abs(math.dot(pivot - cameraPosition, math.mul(cameraRotation, new float3(0f, 0f, 1f))));
		float num2 = cameraDepthToPixelSize.x * num + cameraDepthToPixelSize.y;
		float num3 = size;
		if (sizeIsInPixels)
		{
			num3 *= num2;
		}
		right *= num3;
		up *= num3;
		float x = 0f;
		float num4 = 0f;
		float num5 = 1f;
		for (int i = 0; i < numCharacters; i++)
		{
			ushort num6 = text[i];
			if (num6 == ushort.MaxValue)
			{
				x = math.max(x, num4);
				num4 = 0f;
				num5 += 1f;
			}
			else
			{
				num4 += characterInfo[(int)num6].advance;
			}
		}
		x = math.max(x, num4);
		float3 @float = pivot;
		@float -= right * x * alignment.relativePivot.x;
		float start = 1f - num5;
		float end = 0.75f;
		float num7 = math.lerp(start, end, alignment.relativePivot.y);
		@float -= up * num7;
		@float += math.mul(cameraRotation, new float3(1f, 0f, 0f)) * (num2 * alignment.pixelOffset.x);
		@float += math.mul(cameraRotation, new float3(0f, 1f, 0f)) * (num2 * alignment.pixelOffset.y);
		UnsafeAppendBuffer* ptr = &buffers->textVertices;
		UnsafeAppendBuffer* buffer = &buffers->textTriangles;
		Reserve(ptr, numCharacters * 4 * UnsafeUtility.SizeOf<TextVertex>());
		Reserve(buffer, numCharacters * 6 * UnsafeUtility.SizeOf<int>());
		float3 float2 = @float;
		for (int j = 0; j < numCharacters; j++)
		{
			ushort num8 = text[j];
			if (num8 == ushort.MaxValue)
			{
				float2 -= up;
				@float = float2;
				continue;
			}
			SDFCharacter sDFCharacter = characterInfo[(int)num8];
			int num9 = ptr->Length / UnsafeUtility.SizeOf<TextVertex>();
			float3 float3 = @float + sDFCharacter.vertexTopLeft.x * right + sDFCharacter.vertexTopLeft.y * up;
			minBounds = math.min(minBounds, float3);
			maxBounds = math.max(maxBounds, float3);
			Add(ptr, new TextVertex
			{
				position = float3,
				uv = sDFCharacter.uvTopLeft,
				color = color
			});
			float3 = @float + sDFCharacter.vertexTopRight.x * right + sDFCharacter.vertexTopRight.y * up;
			minBounds = math.min(minBounds, float3);
			maxBounds = math.max(maxBounds, float3);
			Add(ptr, new TextVertex
			{
				position = float3,
				uv = sDFCharacter.uvTopRight,
				color = color
			});
			float3 = @float + sDFCharacter.vertexBottomRight.x * right + sDFCharacter.vertexBottomRight.y * up;
			minBounds = math.min(minBounds, float3);
			maxBounds = math.max(maxBounds, float3);
			Add(ptr, new TextVertex
			{
				position = float3,
				uv = sDFCharacter.uvBottomRight,
				color = color
			});
			float3 = @float + sDFCharacter.vertexBottomLeft.x * right + sDFCharacter.vertexBottomLeft.y * up;
			minBounds = math.min(minBounds, float3);
			maxBounds = math.max(maxBounds, float3);
			Add(ptr, new TextVertex
			{
				position = float3,
				uv = sDFCharacter.uvBottomLeft,
				color = color
			});
			Add(buffer, num9);
			Add(buffer, num9 + 1);
			Add(buffer, num9 + 2);
			Add(buffer, num9);
			Add(buffer, num9 + 2);
			Add(buffer, num9 + 3);
			@float += right * sDFCharacter.advance;
		}
	}

	private unsafe void AddLine(CommandBuilder.LineData line)
	{
		float3 @float = PerspectiveDivide(math.mul(currentMatrix, new float4(line.a, 1f)));
		float3 float2 = PerspectiveDivide(math.mul(currentMatrix, new float4(line.b, 1f)));
		float pixels = currentLineWidthData.pixels;
		float3 float3 = math.normalizesafe(float2 - @float);
		if (math.any(math.isnan(float3)))
		{
			throw new Exception("Nan line coordinates");
		}
		if (pixels <= 0f)
		{
			return;
		}
		minBounds = math.min(minBounds, math.min(@float, float2));
		maxBounds = math.max(maxBounds, math.max(@float, float2));
		UnsafeAppendBuffer* ptr = &buffers->vertices;
		Reserve(ptr, 4 * UnsafeUtility.SizeOf<Vertex>());
		Vertex* ptr2 = (Vertex*)(ptr->Ptr + ptr->Length);
		float3 uv = float3 * pixels;
		float3 uv2 = float3 * pixels;
		if (pixels > 1f && currentLineWidthData.automaticJoins && ptr->Length > 2 * UnsafeUtility.SizeOf<Vertex>())
		{
			Vertex* ptr3 = ptr2 - 1;
			Vertex* ptr4 = ptr2 - 2;
			float num = math.dot(float3, lastNormalizedLineDir);
			if (math.all(ptr4->position == @float) && lastLineWidth == pixels && num >= -0.6f)
			{
				uv = (ptr4->uv2 = (ptr3->uv2 = (float3 + lastNormalizedLineDir) * pixels / (1f + num)));
			}
		}
		ptr->Length += 4 * UnsafeUtility.SizeOf<Vertex>();
		*(ptr2++) = new Vertex
		{
			position = @float,
			color = currentColor,
			uv = new float2(0f, 0f),
			uv2 = uv
		};
		*(ptr2++) = new Vertex
		{
			position = @float,
			color = currentColor,
			uv = new float2(1f, 0f),
			uv2 = uv
		};
		*(ptr2++) = new Vertex
		{
			position = float2,
			color = currentColor,
			uv = new float2(0f, 1f),
			uv2 = uv2
		};
		*(ptr2++) = new Vertex
		{
			position = float2,
			color = currentColor,
			uv = new float2(1f, 1f),
			uv2 = uv2
		};
		lastNormalizedLineDir = float3;
		lastLineWidth = pixels;
	}

	internal static int CircleSteps(float3 center, float radius, float maxPixelError, ref float4x4 currentMatrix, float2 cameraDepthToPixelSize, float3 cameraPosition)
	{
		float4 p = math.mul(currentMatrix, new float4(center, 1f));
		if (math.abs(p.w) < 1E-07f)
		{
			return 3;
		}
		float3 @float = PerspectiveDivide(p);
		float num = math.sqrt(math.max(math.max(math.lengthsq(currentMatrix.c0.xyz), math.lengthsq(currentMatrix.c1.xyz)), math.lengthsq(currentMatrix.c2.xyz))) / p.w;
		float num2 = radius * num;
		float num3 = math.length(@float - cameraPosition);
		float num4 = cameraDepthToPixelSize.x * num3 + cameraDepthToPixelSize.y;
		float num5 = 1f - maxPixelError * num4 / num2;
		if (!(num5 < 0f))
		{
			return (int)math.ceil(MathF.PI / math.acos(num5));
		}
		return 3;
	}

	private void AddCircle(CommandBuilder.CircleData circle)
	{
		if (!math.all(circle.normal == 0f))
		{
			circle.normal = math.normalize(circle.normal);
			if (circle.normal.y < 0f)
			{
				circle.normal = -circle.normal;
			}
			float3 @float = ((!math.all(math.abs(circle.normal - new float3(0f, 1f, 0f)) < 0.001f)) ? math.normalizesafe(math.cross(circle.normal, new float3(0f, 1f, 0f))) : new float3(0f, 0f, 1f));
			float3 float2 = @float;
			float3 normal = circle.normal;
			float3 xyz = math.cross(normal, float2);
			float4x4 float4x = currentMatrix;
			currentMatrix = math.mul(currentMatrix, new float4x4(new float4(float2, 0f) * circle.radius, new float4(normal, 0f) * circle.radius, new float4(xyz, 0f) * circle.radius, new float4(circle.center, 1f)));
			AddCircle(new CommandBuilder.CircleXZData
			{
				center = new float3(0f, 0f, 0f),
				radius = 1f,
				startAngle = 0f,
				endAngle = MathF.PI * 2f
			});
			currentMatrix = float4x;
		}
	}

	private unsafe void AddDisc(CommandBuilder.CircleData circle)
	{
		if (!math.all(circle.normal == 0f))
		{
			int num = CircleSteps(circle.center, circle.radius, maxPixelError, ref currentMatrix, cameraDepthToPixelSize, cameraPosition);
			circle.normal = math.normalize(circle.normal);
			float3 @float = ((!math.all(math.abs(circle.normal - new float3(0f, 1f, 0f)) < 0.001f)) ? math.cross(circle.normal, new float3(0f, 1f, 0f)) : new float3(0f, 0f, 1f));
			float num2 = 1f / (float)num;
			UnsafeAppendBuffer* ptr = &buffers->solidVertices;
			UnsafeAppendBuffer* buffer = &buffers->solidTriangles;
			Reserve(ptr, num * UnsafeUtility.SizeOf<Vertex>());
			Reserve(buffer, 3 * (num - 2) * UnsafeUtility.SizeOf<int>());
			float4x4 a = math.mul(currentMatrix, Matrix4x4.TRS(circle.center, Quaternion.LookRotation(circle.normal, @float), new Vector3(circle.radius, circle.radius, circle.radius)));
			float3 x = minBounds;
			float3 x2 = maxBounds;
			int num3 = ptr->Length / UnsafeUtility.SizeOf<Vertex>();
			for (int i = 0; i < num; i++)
			{
				math.sincos(math.lerp(0f, MathF.PI * 2f, (float)i * num2), out var s, out var c);
				float3 float2 = PerspectiveDivide(math.mul(a, new float4(c, s, 0f, 1f)));
				x = math.min(x, float2);
				x2 = math.max(x2, float2);
				Add(ptr, new Vertex
				{
					position = float2,
					color = currentColor,
					uv = new float2(0f, 0f),
					uv2 = new float3(0f, 0f, 0f)
				});
			}
			minBounds = x;
			maxBounds = x2;
			for (int j = 0; j < num - 2; j++)
			{
				Add(buffer, num3);
				Add(buffer, num3 + j + 1);
				Add(buffer, num3 + j + 2);
			}
		}
	}

	private void AddSphereOutline(CommandBuilder.SphereData circle)
	{
		float4 p = math.mul(currentMatrix, new float4(circle.center, 1f));
		if (math.abs(p.w) < 1E-07f)
		{
			return;
		}
		float3 @float = PerspectiveDivide(p);
		float num = math.sqrt(math.max(math.max(math.lengthsq(currentMatrix.c0.xyz), math.lengthsq(currentMatrix.c1.xyz)), math.lengthsq(currentMatrix.c2.xyz))) / p.w;
		float num2 = circle.radius * num;
		if (cameraIsOrthographic)
		{
			float4x4 float4x = currentMatrix;
			currentMatrix = float4x4.identity;
			AddCircle(new CommandBuilder.CircleData
			{
				center = @float,
				normal = math.mul(cameraRotation, new float3(0f, 0f, 1f)),
				radius = num2
			});
			currentMatrix = float4x;
			return;
		}
		float num3 = math.length(cameraPosition - @float);
		if (!(num3 <= num2))
		{
			float num4 = num2 * num2 / num3;
			float radius = math.sqrt(num2 * num2 - num4 * num4);
			float3 float2 = math.normalize(cameraPosition - @float);
			float4x4 float4x2 = currentMatrix;
			currentMatrix = float4x4.identity;
			AddCircle(new CommandBuilder.CircleData
			{
				center = @float + float2 * num4,
				normal = float2,
				radius = radius
			});
			currentMatrix = float4x2;
		}
	}

	private unsafe void AddCircle(CommandBuilder.CircleXZData circle)
	{
		circle.endAngle = math.clamp(circle.endAngle, circle.startAngle - MathF.PI * 2f, circle.startAngle + MathF.PI * 2f);
		float4x4 a = math.mul(currentMatrix, new float4x4(new float4(circle.radius, 0f, 0f, 0f), new float4(0f, circle.radius, 0f, 0f), new float4(0f, 0f, circle.radius, 0f), new float4(circle.center, 1f)));
		int num = CircleSteps(float3.zero, 1f, maxPixelError, ref a, cameraDepthToPixelSize, cameraPosition);
		float pixels = currentLineWidthData.pixels;
		if (!(pixels < 0f))
		{
			int num2 = num * 4 * UnsafeUtility.SizeOf<Vertex>();
			Reserve(&buffers->vertices, num2);
			Vertex* ptr = (Vertex*)(buffers->vertices.Ptr + buffers->vertices.Length);
			buffers->vertices.Length += num2;
			math.sincos(circle.startAngle, out var s, out var c);
			float3 position = PerspectiveDivide(math.mul(a, new float4(c, 0f, s, 1f)));
			float3 uv = math.normalizesafe(math.mul(a, new float4(0f - s, 0f, c, 0f)).xyz) * pixels;
			float num3 = math.rcp(num);
			for (int i = 1; i <= num; i++)
			{
				math.sincos(math.lerp(circle.startAngle, circle.endAngle, (float)i * num3), out var s2, out var c2);
				float3 @float = PerspectiveDivide(math.mul(a, new float4(c2, 0f, s2, 1f)));
				float3 float2 = math.normalizesafe(math.mul(a, new float4(0f - s2, 0f, c2, 0f)).xyz) * pixels;
				*(ptr++) = new Vertex
				{
					position = position,
					color = currentColor,
					uv = new float2(0f, 0f),
					uv2 = uv
				};
				*(ptr++) = new Vertex
				{
					position = position,
					color = currentColor,
					uv = new float2(1f, 0f),
					uv2 = uv
				};
				*(ptr++) = new Vertex
				{
					position = @float,
					color = currentColor,
					uv = new float2(0f, 1f),
					uv2 = float2
				};
				*(ptr++) = new Vertex
				{
					position = @float,
					color = currentColor,
					uv = new float2(1f, 1f),
					uv2 = float2
				};
				position = @float;
				uv = float2;
			}
			float3 x = PerspectiveDivide(math.mul(a, new float4(-1f, 0f, 0f, 1f)));
			float3 y = PerspectiveDivide(math.mul(a, new float4(0f, -1f, 0f, 1f)));
			float3 y2 = PerspectiveDivide(math.mul(a, new float4(1f, 0f, 0f, 1f)));
			float3 y3 = PerspectiveDivide(math.mul(a, new float4(0f, 1f, 0f, 1f)));
			minBounds = math.min(math.min(math.min(math.min(x, y), y2), y3), minBounds);
			maxBounds = math.max(math.max(math.max(math.max(x, y), y2), y3), maxBounds);
		}
	}

	private unsafe void AddDisc(CommandBuilder.CircleXZData circle)
	{
		int num = CircleSteps(circle.center, circle.radius, maxPixelError, ref currentMatrix, cameraDepthToPixelSize, cameraPosition);
		circle.endAngle = math.clamp(circle.endAngle, circle.startAngle - MathF.PI * 2f, circle.startAngle + MathF.PI * 2f);
		float num2 = 1f / (float)num;
		UnsafeAppendBuffer* ptr = &buffers->solidVertices;
		UnsafeAppendBuffer* buffer = &buffers->solidTriangles;
		Reserve(ptr, (2 + num) * UnsafeUtility.SizeOf<Vertex>());
		Reserve(buffer, 3 * num * UnsafeUtility.SizeOf<int>());
		float4x4 a = math.mul(currentMatrix, Matrix4x4.Translate(circle.center) * Matrix4x4.Scale(new Vector3(circle.radius, circle.radius, circle.radius)));
		float3 @float = PerspectiveDivide(math.mul(a, new float4(0f, 0f, 0f, 1f)));
		Add(ptr, new Vertex
		{
			position = @float,
			color = currentColor,
			uv = new float2(0f, 0f),
			uv2 = new float3(0f, 0f, 0f)
		});
		float3 x = math.min(minBounds, @float);
		float3 x2 = math.max(maxBounds, @float);
		int num3 = ptr->Length / UnsafeUtility.SizeOf<Vertex>();
		for (int i = 0; i <= num; i++)
		{
			math.sincos(math.lerp(circle.startAngle, circle.endAngle, (float)i * num2), out var s, out var c);
			float3 float2 = PerspectiveDivide(math.mul(a, new float4(c, 0f, s, 1f)));
			x = math.min(x, float2);
			x2 = math.max(x2, float2);
			Add(ptr, new Vertex
			{
				position = float2,
				color = currentColor,
				uv = new float2(0f, 0f),
				uv2 = new float3(0f, 0f, 0f)
			});
		}
		minBounds = x;
		maxBounds = x2;
		for (int j = 0; j < num; j++)
		{
			Add(buffer, num3 - 1);
			Add(buffer, num3 + j);
			Add(buffer, num3 + j + 1);
		}
	}

	private unsafe void AddSolidTriangle(CommandBuilder.TriangleData triangle)
	{
		UnsafeAppendBuffer* num = &buffers->solidVertices;
		UnsafeAppendBuffer* buffer = &buffers->solidTriangles;
		Reserve(num, 3 * UnsafeUtility.SizeOf<Vertex>());
		Reserve(buffer, 3 * UnsafeUtility.SizeOf<int>());
		float4x4 a = currentMatrix;
		float3 @float = PerspectiveDivide(math.mul(a, new float4(triangle.a, 1f)));
		float3 float2 = PerspectiveDivide(math.mul(a, new float4(triangle.b, 1f)));
		float3 float3 = PerspectiveDivide(math.mul(a, new float4(triangle.c, 1f)));
		int num2 = num->Length / UnsafeUtility.SizeOf<Vertex>();
		minBounds = math.min(math.min(math.min(minBounds, @float), float2), float3);
		maxBounds = math.max(math.max(math.max(maxBounds, @float), float2), float3);
		Add(num, new Vertex
		{
			position = @float,
			color = currentColor,
			uv = new float2(0f, 0f),
			uv2 = new float3(0f, 0f, 0f)
		});
		Add(num, new Vertex
		{
			position = float2,
			color = currentColor,
			uv = new float2(0f, 0f),
			uv2 = new float3(0f, 0f, 0f)
		});
		Add(num, new Vertex
		{
			position = float3,
			color = currentColor,
			uv = new float2(0f, 0f),
			uv2 = new float3(0f, 0f, 0f)
		});
		Add(buffer, num2);
		Add(buffer, num2 + 1);
		Add(buffer, num2 + 2);
	}

	private void AddWireBox(CommandBuilder.BoxData box)
	{
		float3 @float = box.center - box.size * 0.5f;
		float3 float2 = box.center + box.size * 0.5f;
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(@float.x, @float.y, @float.z),
			b = new float3(float2.x, @float.y, @float.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(float2.x, @float.y, @float.z),
			b = new float3(float2.x, @float.y, float2.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(float2.x, @float.y, float2.z),
			b = new float3(@float.x, @float.y, float2.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(@float.x, @float.y, float2.z),
			b = new float3(@float.x, @float.y, @float.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(@float.x, float2.y, @float.z),
			b = new float3(float2.x, float2.y, @float.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(float2.x, float2.y, @float.z),
			b = new float3(float2.x, float2.y, float2.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(float2.x, float2.y, float2.z),
			b = new float3(@float.x, float2.y, float2.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(@float.x, float2.y, float2.z),
			b = new float3(@float.x, float2.y, @float.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(@float.x, @float.y, @float.z),
			b = new float3(@float.x, float2.y, @float.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(float2.x, @float.y, @float.z),
			b = new float3(float2.x, float2.y, @float.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(float2.x, @float.y, float2.z),
			b = new float3(float2.x, float2.y, float2.z)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(@float.x, @float.y, float2.z),
			b = new float3(@float.x, float2.y, float2.z)
		});
	}

	private void AddPlane(CommandBuilder.PlaneData plane)
	{
		float4x4 float4x = currentMatrix;
		currentMatrix = math.mul(currentMatrix, float4x4.TRS(plane.center, plane.rotation, new float3(plane.size.x * 0.5f, 1f, plane.size.y * 0.5f)));
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(-1f, 0f, -1f),
			b = new float3(1f, 0f, -1f)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(1f, 0f, -1f),
			b = new float3(1f, 0f, 1f)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(1f, 0f, 1f),
			b = new float3(-1f, 0f, 1f)
		});
		AddLine(new CommandBuilder.LineData
		{
			a = new float3(-1f, 0f, 1f),
			b = new float3(-1f, 0f, -1f)
		});
		currentMatrix = float4x;
	}

	private unsafe void AddBox(CommandBuilder.BoxData box)
	{
		UnsafeAppendBuffer* ptr = &buffers->solidVertices;
		UnsafeAppendBuffer* ptr2 = &buffers->solidTriangles;
		Reserve(ptr, BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>());
		Reserve(ptr2, BoxTriangles.Length * UnsafeUtility.SizeOf<int>());
		float3 @float = box.size * 0.5f;
		float4x4 a = math.mul(currentMatrix, new float4x4(new float4(@float.x, 0f, 0f, 0f), new float4(0f, @float.y, 0f, 0f), new float4(0f, 0f, @float.z, 0f), new float4(box.center, 1f)));
		float3 x = minBounds;
		float3 x2 = maxBounds;
		int num = ptr->Length / UnsafeUtility.SizeOf<Vertex>();
		Vertex* ptr3 = (Vertex*)(ptr->Ptr + ptr->Length);
		for (int i = 0; i < BoxVertices.Length; i++)
		{
			float3 float2 = PerspectiveDivide(math.mul(a, BoxVertices[i]));
			x = math.min(x, float2);
			x2 = math.max(x2, float2);
			*(ptr3++) = new Vertex
			{
				position = float2,
				color = currentColor,
				uv = new float2(0f, 0f),
				uv2 = new float3(0f, 0f, 0f)
			};
		}
		ptr->Length += BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>();
		minBounds = x;
		maxBounds = x2;
		int* ptr4 = (int*)(ptr2->Ptr + ptr2->Length);
		for (int j = 0; j < BoxTriangles.Length; j++)
		{
			*(ptr4++) = num + BoxTriangles[j];
		}
		ptr2->Length += BoxTriangles.Length * UnsafeUtility.SizeOf<int>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Next(ref UnsafeAppendBuffer.Reader reader, ref NativeArray<float4x4> matrixStack, ref NativeArray<Color32> colorStack, ref NativeArray<CommandBuilder.LineWidthData> lineWidthStack, ref int matrixStackSize, ref int colorStackSize, ref int lineWidthStackSize)
	{
		CommandBuilder.Command command = reader.ReadNext<CommandBuilder.Command>();
		CommandBuilder.Command command2 = command & (CommandBuilder.Command)255;
		Color32 color = default(Color32);
		if ((command & CommandBuilder.Command.PushColorInline) != 0)
		{
			color = currentColor;
			currentColor = reader.ReadNext<Color32>();
		}
		switch (command2)
		{
		case CommandBuilder.Command.PushColor:
			if (colorStackSize >= colorStack.Length)
			{
				colorStackSize--;
			}
			colorStack[colorStackSize] = currentColor;
			colorStackSize++;
			currentColor = reader.ReadNext<Color32>();
			break;
		case CommandBuilder.Command.PopColor:
			if (colorStackSize > 0)
			{
				colorStackSize--;
				currentColor = colorStack[colorStackSize];
			}
			break;
		case CommandBuilder.Command.PushMatrix:
			if (matrixStackSize >= matrixStack.Length)
			{
				matrixStackSize--;
			}
			matrixStack[matrixStackSize] = currentMatrix;
			matrixStackSize++;
			currentMatrix = math.mul(currentMatrix, reader.ReadNext<float4x4>());
			break;
		case CommandBuilder.Command.PushSetMatrix:
			if (matrixStackSize >= matrixStack.Length)
			{
				matrixStackSize--;
			}
			matrixStack[matrixStackSize] = currentMatrix;
			matrixStackSize++;
			currentMatrix = reader.ReadNext<float4x4>();
			break;
		case CommandBuilder.Command.PopMatrix:
			if (matrixStackSize > 0)
			{
				matrixStackSize--;
				currentMatrix = matrixStack[matrixStackSize];
			}
			break;
		case CommandBuilder.Command.PushLineWidth:
			if (lineWidthStackSize >= lineWidthStack.Length)
			{
				lineWidthStackSize--;
			}
			lineWidthStack[lineWidthStackSize] = currentLineWidthData;
			lineWidthStackSize++;
			currentLineWidthData = reader.ReadNext<CommandBuilder.LineWidthData>();
			currentLineWidthData.pixels *= lineWidthMultiplier;
			break;
		case CommandBuilder.Command.PopLineWidth:
			if (lineWidthStackSize > 0)
			{
				lineWidthStackSize--;
				currentLineWidthData = lineWidthStack[lineWidthStackSize];
			}
			break;
		case CommandBuilder.Command.Line:
			AddLine(reader.ReadNext<CommandBuilder.LineData>());
			break;
		case CommandBuilder.Command.SphereOutline:
			AddSphereOutline(reader.ReadNext<CommandBuilder.SphereData>());
			break;
		case CommandBuilder.Command.CircleXZ:
			AddCircle(reader.ReadNext<CommandBuilder.CircleXZData>());
			break;
		case CommandBuilder.Command.Circle:
			AddCircle(reader.ReadNext<CommandBuilder.CircleData>());
			break;
		case CommandBuilder.Command.DiscXZ:
			AddDisc(reader.ReadNext<CommandBuilder.CircleXZData>());
			break;
		case CommandBuilder.Command.Disc:
			AddDisc(reader.ReadNext<CommandBuilder.CircleData>());
			break;
		case CommandBuilder.Command.Box:
			AddBox(reader.ReadNext<CommandBuilder.BoxData>());
			break;
		case CommandBuilder.Command.WirePlane:
			AddPlane(reader.ReadNext<CommandBuilder.PlaneData>());
			break;
		case CommandBuilder.Command.WireBox:
			AddWireBox(reader.ReadNext<CommandBuilder.BoxData>());
			break;
		case CommandBuilder.Command.SolidTriangle:
			AddSolidTriangle(reader.ReadNext<CommandBuilder.TriangleData>());
			break;
		case CommandBuilder.Command.PushPersist:
			reader.ReadNext<CommandBuilder.PersistData>();
			break;
		case CommandBuilder.Command.Text:
		{
			CommandBuilder.TextData textData2 = reader.ReadNext<CommandBuilder.TextData>();
			ushort* text2 = (ushort*)reader.ReadNext(UnsafeUtility.SizeOf<ushort>() * textData2.numCharacters);
			AddText(text2, textData2, currentColor);
			break;
		}
		case CommandBuilder.Command.Text3D:
		{
			CommandBuilder.TextData3D textData = reader.ReadNext<CommandBuilder.TextData3D>();
			ushort* text = (ushort*)reader.ReadNext(UnsafeUtility.SizeOf<ushort>() * textData.numCharacters);
			AddText3D(text, textData, currentColor);
			break;
		}
		case CommandBuilder.Command.CaptureState:
			buffers->capturedState.Add(new DrawingData.ProcessedBuilderData.CapturedState
			{
				color = currentColor,
				matrix = currentMatrix
			});
			break;
		}
		if ((command & CommandBuilder.Command.PushColorInline) != 0)
		{
			currentColor = color;
		}
	}

	private unsafe void CreateTriangles()
	{
		UnsafeAppendBuffer* num = &buffers->vertices;
		UnsafeAppendBuffer* ptr = &buffers->triangles;
		int num2 = num->Length / UnsafeUtility.SizeOf<Vertex>() / 4;
		int num3 = num2 * 6 * UnsafeUtility.SizeOf<int>();
		if (num3 >= ptr->Capacity)
		{
			ptr->SetCapacity(math.ceilpow2(num3));
		}
		int* ptr2 = (int*)ptr->Ptr;
		int num4 = 0;
		int num5 = 0;
		while (num4 < num2)
		{
			*(ptr2++) = num5;
			*(ptr2++) = num5 + 1;
			*(ptr2++) = num5 + 2;
			*(ptr2++) = num5 + 1;
			*(ptr2++) = num5 + 3;
			*(ptr2++) = num5 + 2;
			num4++;
			num5 += 4;
		}
		ptr->Length = num3;
	}

	public unsafe void Execute()
	{
		buffers->vertices.Reset();
		buffers->triangles.Reset();
		buffers->solidVertices.Reset();
		buffers->solidTriangles.Reset();
		buffers->textVertices.Reset();
		buffers->textTriangles.Reset();
		buffers->capturedState.Reset();
		currentLineWidthData.pixels *= lineWidthMultiplier;
		minBounds = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		maxBounds = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		NativeArray<float4x4> matrixStack = new NativeArray<float4x4>(32, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeArray<Color32> colorStack = new NativeArray<Color32>(32, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeArray<CommandBuilder.LineWidthData> lineWidthStack = new NativeArray<CommandBuilder.LineWidthData>(32, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		int matrixStackSize = 0;
		int colorStackSize = 0;
		int lineWidthStackSize = 0;
		UnsafeAppendBuffer.Reader reader = buffers->splitterOutput.AsReader();
		while (reader.Offset < reader.Size)
		{
			Next(ref reader, ref matrixStack, ref colorStack, ref lineWidthStack, ref matrixStackSize, ref colorStackSize, ref lineWidthStackSize);
		}
		CreateTriangles();
		Bounds* ptr = &buffers->bounds;
		*ptr = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);
		if (math.any(math.isnan(ptr->min)) && (buffers->vertices.Length > 0 || buffers->solidTriangles.Length > 0))
		{
			*ptr = new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
		}
	}
}
