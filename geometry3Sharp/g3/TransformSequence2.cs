using System;
using System.Collections.Generic;

namespace g3;

public class TransformSequence2 : ITransform2
{
	private enum XFormType
	{
		Translation = 0,
		Rotation = 1,
		RotateAroundPoint = 2,
		Scale = 3,
		ScaleAroundPoint = 4,
		NestedITransform2 = 10
	}

	private struct XForm
	{
		public XFormType type;

		public Vector2dTuple2 data;

		public object xform;

		public Vector2d Translation => data.V0;

		public Vector2d Scale => data.V0;

		public Matrix2d Rotation => new Matrix2d(data.V0.x);

		public Vector2d RotateOrigin => data.V1;

		public bool ScaleIsUniform => data.V0.EpsilonEqual(data.V1, 1.1920928955078125E-07);

		public ITransform2 NestedITransform2 => xform as ITransform2;
	}

	private List<XForm> Operations;

	public TransformSequence2()
	{
		Operations = new List<XForm>();
	}

	public TransformSequence2 Translation(Vector2d dv)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Translation,
			data = new Vector2dTuple2(dv, Vector2d.Zero)
		});
		return this;
	}

	public TransformSequence2 Translation(double dx, double dy)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Translation,
			data = new Vector2dTuple2(new Vector2d(dx, dy), Vector2d.Zero)
		});
		return this;
	}

	public TransformSequence2 RotationRad(double angle)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Rotation,
			data = new Vector2dTuple2(new Vector2d(angle, 0.0), Vector2d.Zero)
		});
		return this;
	}

	public TransformSequence2 RotationDeg(double angle)
	{
		return RotationRad(angle * (Math.PI / 180.0));
	}

	public TransformSequence2 RotationRad(double angle, Vector2d aroundPt)
	{
		Operations.Add(new XForm
		{
			type = XFormType.RotateAroundPoint,
			data = new Vector2dTuple2(new Vector2d(angle, 0.0), aroundPt)
		});
		return this;
	}

	public TransformSequence2 RotationDeg(double angle, Vector2d aroundPt)
	{
		return RotationRad(angle * (Math.PI / 180.0), aroundPt);
	}

	public TransformSequence2 Scale(Vector2d s)
	{
		Operations.Add(new XForm
		{
			type = XFormType.Scale,
			data = new Vector2dTuple2(s, Vector2d.Zero)
		});
		return this;
	}

	public TransformSequence2 Scale(Vector2d s, Vector2d aroundPt)
	{
		Operations.Add(new XForm
		{
			type = XFormType.ScaleAroundPoint,
			data = new Vector2dTuple2(s, aroundPt)
		});
		return this;
	}

	public TransformSequence2 Append(ITransform2 t2)
	{
		Operations.Add(new XForm
		{
			type = XFormType.NestedITransform2,
			xform = t2
		});
		return this;
	}

	public Vector2d TransformP(Vector2d p)
	{
		int count = Operations.Count;
		for (int i = 0; i < count; i++)
		{
			switch (Operations[i].type)
			{
			case XFormType.Translation:
				p += Operations[i].Translation;
				break;
			case XFormType.Rotation:
				p = Operations[i].Rotation * p;
				break;
			case XFormType.RotateAroundPoint:
				p -= Operations[i].RotateOrigin;
				p = Operations[i].Rotation * p;
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
			case XFormType.NestedITransform2:
				p = Operations[i].NestedITransform2.TransformP(p);
				break;
			default:
				throw new NotImplementedException("TransformSequence.TransformP: unhandled type!");
			}
		}
		return p;
	}

	public Vector2d TransformN(Vector2d n)
	{
		int count = Operations.Count;
		for (int i = 0; i < count; i++)
		{
			switch (Operations[i].type)
			{
			case XFormType.Rotation:
				n = Operations[i].Rotation * n;
				break;
			case XFormType.RotateAroundPoint:
				n = Operations[i].Rotation * n;
				break;
			case XFormType.Scale:
				n *= Operations[i].Scale;
				break;
			case XFormType.ScaleAroundPoint:
				n *= Operations[i].Scale;
				break;
			case XFormType.NestedITransform2:
				n = Operations[i].NestedITransform2.TransformN(n);
				break;
			default:
				throw new NotImplementedException("TransformSequence.TransformN: unhandled type!");
			case XFormType.Translation:
				break;
			}
		}
		return n;
	}

	public double TransformScalar(double s)
	{
		int count = Operations.Count;
		for (int i = 0; i < count; i++)
		{
			switch (Operations[i].type)
			{
			case XFormType.Scale:
				s *= Operations[i].Scale.x;
				break;
			case XFormType.ScaleAroundPoint:
				s *= Operations[i].Scale.x;
				break;
			case XFormType.NestedITransform2:
				s = Operations[i].NestedITransform2.TransformScalar(s);
				break;
			default:
				throw new NotImplementedException("TransformSequence.TransformScalar: unhandled type!");
			case XFormType.Translation:
			case XFormType.Rotation:
			case XFormType.RotateAroundPoint:
				break;
			}
		}
		return s;
	}
}
