using System;
using System.Runtime.InteropServices;

public class TreeNodeDefinition : MothershipResponse
{
	private HandleRef swigCPtr;

	public string id
	{
		get
		{
			string result = MothershipApiPINVOKE.TreeNodeDefinition_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string tree_id
	{
		get
		{
			string result = MothershipApiPINVOKE.TreeNodeDefinition_tree_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_tree_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string name
	{
		get
		{
			string result = MothershipApiPINVOKE.TreeNodeDefinition_name_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_name_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public ComplexPrerequisiteNodes prerequisite_nodes
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.TreeNodeDefinition_prerequisite_nodes_get(swigCPtr);
			ComplexPrerequisiteNodes result = ((intPtr == IntPtr.Zero) ? null : new ComplexPrerequisiteNodes(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_prerequisite_nodes_set(swigCPtr, ComplexPrerequisiteNodes.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public ListEntitlementResultsVector prerequisite_entitlements
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.TreeNodeDefinition_prerequisite_entitlements_get(swigCPtr);
			ListEntitlementResultsVector result = ((intPtr == IntPtr.Zero) ? null : new ListEntitlementResultsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_prerequisite_entitlements_set(swigCPtr, ListEntitlementResultsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public PrerequisiteLevelVector prerequisite_levels
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.TreeNodeDefinition_prerequisite_levels_get(swigCPtr);
			PrerequisiteLevelVector result = ((intPtr == IntPtr.Zero) ? null : new PrerequisiteLevelVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_prerequisite_levels_set(swigCPtr, PrerequisiteLevelVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public HydratedProgressionNodeCost cost
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.TreeNodeDefinition_cost_get(swigCPtr);
			HydratedProgressionNodeCost result = ((intPtr == IntPtr.Zero) ? null : new HydratedProgressionNodeCost(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_cost_set(swigCPtr, HydratedProgressionNodeCost.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public MothershipHydratedTransactionCatalogItem transaction
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.TreeNodeDefinition_transaction_get(swigCPtr);
			MothershipHydratedTransactionCatalogItem result = ((intPtr == IntPtr.Zero) ? null : new MothershipHydratedTransactionCatalogItem(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.TreeNodeDefinition_transaction_set(swigCPtr, MothershipHydratedTransactionCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal TreeNodeDefinition(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.TreeNodeDefinition_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(TreeNodeDefinition obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(TreeNodeDefinition obj)
	{
		if (obj != null)
		{
			if (!obj.swigCMemOwn)
			{
				throw new ApplicationException("Cannot release ownership as memory is not owned");
			}
			HandleRef result = obj.swigCPtr;
			obj.swigCMemOwn = false;
			obj.Dispose();
			return result;
		}
		return new HandleRef(null, IntPtr.Zero);
	}

	protected override void Dispose(bool disposing)
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					MothershipApiPINVOKE.delete_TreeNodeDefinition(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public static TreeNodeDefinition FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.TreeNodeDefinition_FromMothershipResponse(MothershipResponse.getCPtr(response));
		TreeNodeDefinition result = ((intPtr == IntPtr.Zero) ? null : new TreeNodeDefinition(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ParseFromString(string response)
	{
		bool result = MothershipApiPINVOKE.TreeNodeDefinition_ParseFromString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ToJson(SWIGTYPE_p_rapidjson__Value treeNode, SWIGTYPE_p_rapidjson__Document body)
	{
		bool result = MothershipApiPINVOKE.TreeNodeDefinition_ToJson(swigCPtr, SWIGTYPE_p_rapidjson__Value.getCPtr(treeNode), SWIGTYPE_p_rapidjson__Document.getCPtr(body));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public TreeNodeDefinition()
		: this(MothershipApiPINVOKE.new_TreeNodeDefinition(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
