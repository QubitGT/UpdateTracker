using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalOpus.MB.Core;

[Serializable]
public class MB3_MeshCombinerSettingsData : MB_IMeshBakerSettings
{
	[SerializeField]
	protected MB_RenderType _renderType;

	[SerializeField]
	protected MB2_OutputOptions _outputOption;

	[SerializeField]
	protected MB2_LightmapOptions _lightmapOption = MB2_LightmapOptions.ignore_UV2;

	[SerializeField]
	protected bool _doNorm = true;

	[SerializeField]
	protected bool _doTan = true;

	[SerializeField]
	protected bool _doCol;

	[SerializeField]
	protected bool _doUV = true;

	[SerializeField]
	protected bool _doUV3;

	[SerializeField]
	protected bool _doUV4;

	[SerializeField]
	protected bool _doUV5;

	[SerializeField]
	protected bool _doUV6;

	[SerializeField]
	protected bool _doUV7;

	[SerializeField]
	protected bool _doUV8;

	[SerializeField]
	protected bool _doBlendShapes;

	[FormerlySerializedAs("_recenterVertsToBoundsCenter")]
	[SerializeField]
	protected MB_MeshPivotLocation _pivotLocationType;

	[SerializeField]
	protected Vector3 _pivotLocation;

	[SerializeField]
	protected bool _clearBuffersAfterBake;

	[SerializeField]
	public bool _optimizeAfterBake = true;

	[SerializeField]
	protected float _uv2UnwrappingParamsHardAngle = 60f;

	[SerializeField]
	protected float _uv2UnwrappingParamsPackMargin = 0.005f;

	[SerializeField]
	protected bool _smrNoExtraBonesWhenCombiningMeshRenderers;

	[SerializeField]
	protected bool _smrMergeBlendShapesWithSameNames;

	[SerializeField]
	protected UnityEngine.Object _assignToMeshCustomizer;

	[SerializeField]
	protected MB_MeshCombineAPIType _meshAPItoUse = MB_MeshCombineAPIType.betaNativeArrayAPI;

	public virtual MB_RenderType renderType
	{
		get
		{
			return _renderType;
		}
		set
		{
			_renderType = value;
		}
	}

	public virtual MB2_OutputOptions outputOption
	{
		get
		{
			return _outputOption;
		}
		set
		{
			_outputOption = value;
		}
	}

	public virtual MB2_LightmapOptions lightmapOption
	{
		get
		{
			return _lightmapOption;
		}
		set
		{
			_lightmapOption = value;
		}
	}

	public virtual bool doNorm
	{
		get
		{
			return _doNorm;
		}
		set
		{
			_doNorm = value;
		}
	}

	public virtual bool doTan
	{
		get
		{
			return _doTan;
		}
		set
		{
			_doTan = value;
		}
	}

	public virtual bool doCol
	{
		get
		{
			return _doCol;
		}
		set
		{
			_doCol = value;
		}
	}

	public virtual bool doUV
	{
		get
		{
			return _doUV;
		}
		set
		{
			_doUV = value;
		}
	}

	public virtual bool doUV3
	{
		get
		{
			return _doUV3;
		}
		set
		{
			_doUV3 = value;
		}
	}

	public virtual bool doUV4
	{
		get
		{
			return _doUV4;
		}
		set
		{
			_doUV4 = value;
		}
	}

	public virtual bool doUV5
	{
		get
		{
			return _doUV5;
		}
		set
		{
			_doUV5 = value;
		}
	}

	public virtual bool doUV6
	{
		get
		{
			return _doUV6;
		}
		set
		{
			_doUV6 = value;
		}
	}

	public virtual bool doUV7
	{
		get
		{
			return _doUV7;
		}
		set
		{
			_doUV7 = value;
		}
	}

	public virtual bool doUV8
	{
		get
		{
			return _doUV8;
		}
		set
		{
			_doUV8 = value;
		}
	}

	public virtual bool doBlendShapes
	{
		get
		{
			return _doBlendShapes;
		}
		set
		{
			_doBlendShapes = value;
		}
	}

	public virtual MB_MeshPivotLocation pivotLocationType
	{
		get
		{
			return _pivotLocationType;
		}
		set
		{
			_pivotLocationType = value;
		}
	}

	public virtual Vector3 pivotLocation
	{
		get
		{
			return _pivotLocation;
		}
		set
		{
			_pivotLocation = value;
		}
	}

	public bool clearBuffersAfterBake
	{
		get
		{
			return _clearBuffersAfterBake;
		}
		set
		{
			_clearBuffersAfterBake = value;
		}
	}

	public bool optimizeAfterBake
	{
		get
		{
			return _optimizeAfterBake;
		}
		set
		{
			_optimizeAfterBake = value;
		}
	}

	public float uv2UnwrappingParamsHardAngle
	{
		get
		{
			return _uv2UnwrappingParamsHardAngle;
		}
		set
		{
			_uv2UnwrappingParamsHardAngle = value;
		}
	}

	public float uv2UnwrappingParamsPackMargin
	{
		get
		{
			return _uv2UnwrappingParamsPackMargin;
		}
		set
		{
			_uv2UnwrappingParamsPackMargin = value;
		}
	}

	public bool smrNoExtraBonesWhenCombiningMeshRenderers
	{
		get
		{
			return _smrNoExtraBonesWhenCombiningMeshRenderers;
		}
		set
		{
			_smrNoExtraBonesWhenCombiningMeshRenderers = value;
		}
	}

	public bool smrMergeBlendShapesWithSameNames
	{
		get
		{
			return _smrMergeBlendShapesWithSameNames;
		}
		set
		{
			_smrMergeBlendShapesWithSameNames = value;
		}
	}

	public IAssignToMeshCustomizer assignToMeshCustomizer
	{
		get
		{
			if (_assignToMeshCustomizer is IAssignToMeshCustomizer)
			{
				return (IAssignToMeshCustomizer)_assignToMeshCustomizer;
			}
			_assignToMeshCustomizer = null;
			return null;
		}
		set
		{
			_assignToMeshCustomizer = (UnityEngine.Object)value;
		}
	}

	public MB_MeshCombineAPIType meshAPI
	{
		get
		{
			return _meshAPItoUse;
		}
		set
		{
			_meshAPItoUse = value;
		}
	}
}
