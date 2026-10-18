using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class TransformSequence
{
	private enum XFormType
	{
		Translation,
		QuaterionRotation,
		QuaternionRotateAroundPoint,
		Scale,
		ScaleAroundPoint,
		ToFrame,
		FromFrame
	}

	private struct XForm
	{
		public XFormType type;

		public Vector3dTuple3 data;

		public Vector3d Translation => data.V0;

		public Vector3d Scale => data.V0;

		public Quaternionf Quaternion => new Quaternionf((float)data.V0.x, (float)data.V0.y, (float)data.V0.z, (float)data.V1.x);

		public Vector3d RotateOrigin => data.V2;

		public Frame3f Frame => new Frame3f((Vector3f)RotateOrigin, Quaternion);
	}

	private List<XForm> Operations;

	public TransformSequence()
	{
		Operations = new List<XForm>();
	}

	public TransformSequence(TransformSequence copy)
	{
		Operations = new List<XForm>(copy.Operations);
	}

	public void Append(TransformSequence sequence)
	{
		Operations.AddRange(sequence.Operations);
	}

	public void AppendTranslation(Vector3d dv)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Translation,
			data = new Vector3dTuple3(dv, Vector3d.Zero, Vector3d.Zero)
		});
	}

	public void AppendTranslation(double dx, double dy, double dz)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Translation,
			data = new Vector3dTuple3(new Vector3d(dx, dy, dz), Vector3d.Zero, Vector3d.Zero)
		});
	}

	public void AppendRotation(Quaternionf q)
	{
		Operations.Add(new XForm
		{
			type = XFormType.QuaterionRotation,
			data = new Vector3dTuple3(new Vector3d(q.x, q.y, q.z), new Vector3d(q.w, 0.0, 0.0), Vector3d.Zero)
		});
	}

	public void AppendRotation(Quaternionf q, Vector3d aroundPt)
	{
		Operations.Add(new XForm
		{
			type = XFormType.QuaternionRotateAroundPoint,
			data = new Vector3dTuple3(new Vector3d(q.x, q.y, q.z), new Vector3d(q.w, 0.0, 0.0), aroundPt)
		});
	}

	public void AppendScale(Vector3d s)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Scale,
			data = new Vector3dTuple3(s, Vector3d.Zero, Vector3d.Zero)
		});
	}

	public void AppendScale(Vector3d s, Vector3d aroundPt)
	{
		Operations.Add(new XForm
		{
			type = XFormType.ScaleAroundPoint,
			data = new Vector3dTuple3(s, Vector3d.Zero, aroundPt)
		});
	}

	public void AppendToFrame(Frame3f frame)
	{
		Quaternionf rotation = frame.Rotation;
		Operations.Add(new XForm
		{
			type = XFormType.ToFrame,
			data = new Vector3dTuple3(new Vector3d(rotation.x, rotation.y, rotation.z), new Vector3d(rotation.w, 0.0, 0.0), frame.Origin)
		});
	}

	public void AppendFromFrame(Frame3f frame)
	{
		Quaternionf rotation = frame.Rotation;
		Operations.Add(new XForm
		{
			type = XFormType.FromFrame,
			data = new Vector3dTuple3(new Vector3d(rotation.x, rotation.y, rotation.z), new Vector3d(rotation.w, 0.0, 0.0), frame.Origin)
		});
	}

	public Vector3d TransformP(Vector3d p)
	{
		int count = Operations.Count;
		for (int i = 0; i < count; i++)
		{
			switch (Operations[i].type)
			{
			case XFormType.Translation:
				p += Operations[i].Translation;
				break;
			case XFormType.QuaterionRotation:
				p = Operations[i].Quaternion * p;
				break;
			case XFormType.QuaternionRotateAroundPoint:
				p -= Operations[i].RotateOrigin;
				p = Operations[i].Quaternion * p;
				p += Operations[i].RotateOrigin;
				break;
			case XFormType.Scale:
				p *= Operations[i].Scale;
				break;
			case XFormType.ScaleAroundPoint:
				p -= Operations[i].RotateOrigin;
				p *= Operations[i].Scale;
				p += Operations[i].RotateOrigin;
				break;
			case XFormType.ToFrame:
				p = Operations[i].Frame.ToFrameP(ref p);
				break;
			case XFormType.FromFrame:
				p = Operations[i].Frame.FromFrameP(ref p);
				break;
			default:
				throw new NotImplementedException("TransformSequence.TransformP: unhandled type!");
			}
		}
		return p;
	}

	public Vector3d TransformV(Vector3d v)
	{
		int count = Operations.Count;
		for (int i = 0; i < count; i++)
		{
			switch (Operations[i].type)
			{
			case XFormType.QuaterionRotation:
			case XFormType.QuaternionRotateAroundPoint:
				v = Operations[i].Quaternion * v;
				break;
			case XFormType.Scale:
			case XFormType.ScaleAroundPoint:
				v *= Operations[i].Scale;
				break;
			case XFormType.ToFrame:
				v = Operations[i].Frame.ToFrameV(ref v);
				break;
			case XFormType.FromFrame:
				v = Operations[i].Frame.FromFrameV(ref v);
				break;
			default:
				throw new NotImplementedException("TransformSequence.TransformV: unhandled type!");
			case XFormType.Translation:
				break;
			}
		}
		return v;
	}

	public Vector3f TransformP(Vector3f p)
	{
		return (Vector3f)TransformP((Vector3d)p);
	}

	public TransformSequence MakeInverse()
	{
		TransformSequence transformSequence = new TransformSequence();
		for (int num = Operations.Count - 1; num >= 0; num--)
		{
			switch (Operations[num].type)
			{
			case XFormType.Translation:
				transformSequence.AppendTranslation(-Operations[num].Translation);
				break;
			case XFormType.QuaterionRotation:
				transformSequence.AppendRotation(Operations[num].Quaternion.Inverse());
				break;
			case XFormType.QuaternionRotateAroundPoint:
				transformSequence.AppendRotation(Operations[num].Quaternion.Inverse(), Operations[num].RotateOrigin);
				break;
			case XFormType.Scale:
				transformSequence.AppendScale(1.0 / Operations[num].Scale);
				break;
			case XFormType.ScaleAroundPoint:
				transformSequence.AppendScale(1.0 / Operations[num].Scale, Operations[num].RotateOrigin);
				break;
			case XFormType.ToFrame:
				transformSequence.AppendFromFrame(Operations[num].Frame);
				break;
			case XFormType.FromFrame:
				transformSequence.AppendToFrame(Operations[num].Frame);
				break;
			default:
				throw new NotImplementedException("TransformSequence.MakeInverse: unhandled type!");
			}
		}
		return transformSequence;
	}

	public void Store(BinaryWriter writer)
	{
		writer.Write(1);
		writer.Write(Operations.Count);
		for (int i = 0; i < Operations.Count; i++)
		{
			writer.Write((int)Operations[i].type);
			gSerialization.Store(Operations[i].data.V0, writer);
			gSerialization.Store(Operations[i].data.V1, writer);
			gSerialization.Store(Operations[i].data.V2, writer);
		}
	}

	public void Restore(BinaryReader reader)
	{
		if (reader.ReadInt32() != 1)
		{
			throw new Exception("TransformSequence.Restore: unknown version number!");
		}
		int num = reader.ReadInt32();
		Operations = new List<XForm>();
		for (int i = 0; i < num; i++)
		{
			int type = reader.ReadInt32();
			XForm xForm = default(XForm);
			xForm.type = (XFormType)type;
			XForm item = xForm;
			gSerialization.Restore(ref item.data.V0, reader);
			gSerialization.Restore(ref item.data.V1, reader);
			gSerialization.Restore(ref item.data.V2, reader);
			Operations.Add(item);
		}
	}
}
