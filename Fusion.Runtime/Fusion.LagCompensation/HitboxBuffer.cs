#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fusion.Statistics;
using UnityEngine;

namespace Fusion.LagCompensation;

internal class HitboxBuffer
{
	internal class HitboxSnapshot : IHitboxColliderContainer
	{
		private HitboxCollider[] _colliders;

		private int _collidersCount = 1;

		private int _collidersTempCount = 0;

		private int _collidersFreeHead = 0;

		internal ILagCompensationBroadphase _broadphase;

		internal int Tick;

		internal int DataTick;

		private const int HIGH_COLLIDERS_CAPACITY = 1024;

		internal int CollidersCapacity => _colliders.Length;

		internal int CollidersCount => _collidersCount - 1;

		internal HitboxSnapshot(Mapper mapper, List<HitboxRoot> initialObjects, int hitboxCapacity, float expansionFactor)
		{
			int num = Math.Max(16, hitboxCapacity);
			_colliders = new HitboxCollider[num];
			if (initialObjects != null)
			{
				foreach (HitboxRoot initialObject in initialObjects)
				{
					initialObject.RegisterColliders(this, 0);
				}
			}
			_broadphase = new BVH(mapper, num * 2, initialObjects, expansionFactor);
		}

		internal void CopyFrom(int tick, int dataTick, HitboxSnapshot from)
		{
			ReleaseTempColliders();
			_broadphase.CopyFrom(from._broadphase);
			Tick = tick;
			DataTick = dataTick;
			if (CollidersCapacity < from._collidersCount)
			{
				ResizeCollidersArray(_collidersCount - CollidersCapacity);
			}
			Array.Copy(from._colliders, 0, _colliders, 0, from._collidersCount);
			Array.Clear(_colliders, from._collidersCount, _colliders.Length - from._collidersCount);
			_collidersCount = from._collidersCount;
			_collidersFreeHead = from._collidersFreeHead;
		}

		public ref HitboxCollider GetNextCollider(out int index)
		{
			Assert.Check(_collidersTempCount == 0, "Temp Colliders were not released. {0} {1} {2}", _collidersTempCount, _collidersCount, CollidersCapacity);
			if (_collidersFreeHead == 0)
			{
				if (_collidersCount >= CollidersCapacity)
				{
					ResizeCollidersArray(CollidersCapacity);
				}
				index = _collidersCount++;
			}
			else
			{
				index = _collidersFreeHead;
				_collidersFreeHead = _colliders[_collidersFreeHead].Next;
			}
			Assert.Check(!_colliders[index].Used, index);
			_colliders[index] = default(HitboxCollider);
			_colliders[index].Used = true;
			return ref _colliders[index];
		}

		private void ResizeCollidersArray(int minimumIncrease)
		{
			int num = CollidersCapacity * Math.Max(2, Mathf.FloorToInt((float)minimumIncrease / (float)CollidersCapacity + 1f));
			if (num >= 1024)
			{
				InternalLogStreams.LogDebug?.Warn($"Resizing Hitboxsnapshot colliders capacity from {CollidersCapacity} to {num}, this value appears to be elevated and may not have been intended. It is recommended to check for any potential unnecessary Hitboxes.");
			}
			Array.Resize(ref _colliders, num);
		}

		public ref HitboxCollider GetNextTempCollider(out int tmpIndex)
		{
			if (_collidersCount + _collidersTempCount >= CollidersCapacity)
			{
				ResizeCollidersArray(CollidersCapacity);
			}
			tmpIndex = _collidersCount + _collidersTempCount++;
			Assert.Check(!_colliders[tmpIndex].Used, tmpIndex);
			_colliders[tmpIndex] = default(HitboxCollider);
			return ref _colliders[tmpIndex];
		}

		public void ReleaseTempColliders()
		{
			if (_collidersTempCount > 0)
			{
				Array.Clear(_colliders, _collidersCount, _collidersTempCount);
			}
			_collidersTempCount = 0;
		}

		public void ReleaseCollider(int index)
		{
			if (index <= 0 || index >= CollidersCapacity)
			{
				throw new IndexOutOfRangeException($"Index {index} is out of valid range: (0, {CollidersCapacity})");
			}
			Assert.Check(_colliders[index].Used, index);
			_colliders[index].Used = false;
			_colliders[index].Next = _collidersFreeHead;
			_collidersFreeHead = index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref HitboxCollider GetCollider(int index)
		{
			if (index <= 0 || index >= CollidersCapacity)
			{
				throw new IndexOutOfRangeException($"Index {index} is out of valid range: (0, {CollidersCapacity})");
			}
			return ref _colliders[index];
		}

		internal void Add(HitboxRoot h, LagCompensationStatisticsManager lagCompStatManager)
		{
			Timer timer = Timer.StartNew();
			h.RegisterColliders(this, DataTick);
			timer.Stop();
			lagCompStatManager.PendingSnapshot.SetAddOnBufferTime(timer.ElapsedInMilliseconds);
			Timer timer2 = Timer.StartNew();
			_broadphase.Add(h);
			timer2.Stop();
			lagCompStatManager.PendingSnapshot.SetAddOnBVHTime(timer2.ElapsedInMilliseconds);
		}

		internal bool Remove(HitboxRoot hr)
		{
			hr.DeregisterColliders(this);
			return _broadphase.Remove(hr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Update(HitboxRoot h, LagCompensationStatisticsManager lagCompStatManager)
		{
			Timer timer = Timer.StartNew();
			bool hitboxRootActive = h.HitboxRootActive;
			Hitbox[] hitboxes = h.Hitboxes;
			foreach (Hitbox hitbox in hitboxes)
			{
				ref HitboxCollider collider = ref GetCollider(hitbox.ColliderIndex);
				if (hitboxRootActive)
				{
					hitbox.SetColliderData(ref collider, DataTick);
				}
				else
				{
					collider.Active = false;
				}
			}
			timer.Stop();
			lagCompStatManager.PendingSnapshot.SetUpdateBufferTime(timer.ElapsedInMilliseconds);
			Timer timer2 = Timer.StartNew();
			_broadphase.Update(h, DataTick);
			timer2.Stop();
			lagCompStatManager.PendingSnapshot.SetUpdateBVHTime(timer2.ElapsedInMilliseconds);
		}

		public void QueryBroadphase(Query query, HashSet<HitboxRoot> broadphaseCandidates)
		{
			_broadphase.Traverse(query, broadphaseCandidates, query.LayerMask);
		}

		public static void ProcessBroadphaseRootCandidates(Query query, IHitboxColliderContainer fromContainer, HashSet<HitboxRoot> rootCandidates, HashSet<int> processedColliderIndices, IHitboxColliderContainer toContainer = null)
		{
			bool flag = (query.Options & HitOptions.IgnoreInputAuthority) == HitOptions.IgnoreInputAuthority && query.Player.IsRealPlayer;
			bool flag2 = toContainer != null && (query.Options & HitOptions.SubtickAccuracy) == HitOptions.SubtickAccuracy;
			foreach (HitboxRoot rootCandidate in rootCandidates)
			{
				if (flag && rootCandidate.Object.InputAuthority == query.Player)
				{
					continue;
				}
				Hitbox[] hitboxes = rootCandidate.Hitboxes;
				foreach (Hitbox hitbox in hitboxes)
				{
					int colliderIndex = hitbox.ColliderIndex;
					ref HitboxCollider collider = ref fromContainer.GetCollider(colliderIndex);
					bool flag3 = collider.Active && ((int)query.LayerMask & collider.layerMask) != 0 && collider.Used;
					if (flag3)
					{
						processedColliderIndices.Add(colliderIndex);
					}
					if (!flag2)
					{
						continue;
					}
					ref HitboxCollider collider2 = ref toContainer.GetCollider(colliderIndex);
					if (collider2.Active && collider2.Used && (collider2.layerMask & (int)query.LayerMask) != 0)
					{
						if (!flag3 || collider.Hitbox != collider2.Hitbox)
						{
							fromContainer.GetNextTempCollider(out var tmpIndex) = collider2;
							processedColliderIndices.Add(tmpIndex);
							continue;
						}
						processedColliderIndices.Remove(colliderIndex);
						int tmpIndex2;
						ref HitboxCollider nextTempCollider = ref fromContainer.GetNextTempCollider(out tmpIndex2);
						HitboxCollider.Lerp(ref collider, ref collider2, query.Alpha.Value, ref nextTempCollider);
						processedColliderIndices.Add(tmpIndex2);
					}
				}
			}
		}
	}

	internal HitboxSnapshot[] _buffer;

	private Mapper _mapper;

	private int _head = 0;

	private int _advanced = 0;

	internal int Tick;

	private readonly HashSet<HitboxRoot> _broadphaseCandidates = new HashSet<HitboxRoot>();

	private readonly HashSet<int> _colliderCandidates = new HashSet<int>();

	internal int Length => _buffer.Length;

	internal BVH BVH => _buffer[_head]._broadphase as BVH;

	internal HitboxSnapshot Current => _buffer[_head];

	internal HitboxBuffer(List<HitboxRoot> initialObjects, int bufferSize, int hitboxCapacity, float expansionFactor)
	{
		if (bufferSize <= 0)
		{
			InternalLogStreams.LogDebug?.Warn(string.Format("Trying to initialize {0} with {1} length. Initiatizing with 1 instead.", "HitboxBuffer", bufferSize));
			bufferSize = 1;
		}
		_buffer = new HitboxSnapshot[bufferSize];
		_mapper = new Mapper();
		_head = 0;
		_advanced = 0;
		Assert.Check(Length > 0);
		hitboxCapacity = ((initialObjects != null) ? Math.Max(hitboxCapacity, initialObjects.Count) : hitboxCapacity);
		_buffer[0] = new HitboxSnapshot(_mapper, initialObjects, hitboxCapacity, expansionFactor);
		for (int i = 1; i < Length; i++)
		{
			_buffer[i] = new HitboxSnapshot(_mapper, null, hitboxCapacity, expansionFactor);
		}
	}

	internal void Advance(int tick, int dataTick)
	{
		int num;
		if (tick == Tick)
		{
			num = (_head + _buffer.Length - 1) % _buffer.Length;
		}
		else
		{
			num = _head;
			_advanced++;
		}
		_head = (num + 1) % _buffer.Length;
		_buffer[_head].CopyFrom(tick, dataTick, _buffer[num]);
		Tick = tick;
	}

	internal void PosUpdateRefit()
	{
		BVH.PosUpdateRefit();
	}

	internal void Add(HitboxRoot root, LagCompensationStatisticsManager lagCompStatManager)
	{
		_buffer[_head].Add(root, lagCompStatManager);
	}

	internal bool Remove(HitboxRoot root)
	{
		return _buffer[_head].Remove(root);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Update(HitboxRoot root, LagCompensationStatisticsManager lagCompStatManager)
	{
		_buffer[_head].Update(root, lagCompStatManager);
	}

	internal bool PerformQuery(Query query, List<HitboxHit> hits)
	{
		_colliderCandidates.Clear();
		QueryBroadphase(query, _colliderCandidates, out var container);
		if (_colliderCandidates.Count <= 0)
		{
			return false;
		}
		InitColliderCandidatesForNarrowPhase(container, _colliderCandidates);
		bool result = query.NarrowPhase(container, _colliderCandidates, hits);
		container.ReleaseTempColliders();
		return result;
	}

	internal void PositionQueryInternal(ref PositionRotationQueryParams param, out Vector3 position, out Quaternion rotation)
	{
		GetClosestSnapshotForTick(param.QueryParams.Tick, out var snapshot);
		int colliderIndex = param.Hitbox.ColliderIndex;
		HitboxCollider from = snapshot.GetCollider(colliderIndex);
		if ((param.QueryParams.Options & HitOptions.SubtickAccuracy) == HitOptions.SubtickAccuracy && param.QueryParams.TickTo.HasValue && param.QueryParams.Alpha.HasValue)
		{
			GetClosestSnapshotForTick(param.QueryParams.TickTo.Value, out var snapshot2);
			HitboxCollider.Lerp(ref from, ref snapshot2.GetCollider(colliderIndex), param.QueryParams.Alpha.Value, ref from);
		}
		position = from.Position;
		rotation = QuaternionFromMatrix(from.LocalToWorld);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void InitColliderCandidatesForNarrowPhase(IHitboxColliderContainer container, HashSet<int> candidates)
	{
		foreach (int candidate in candidates)
		{
			container.GetCollider(candidate).InitNarrowData();
		}
	}

	internal static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
	}

	private void GetClosestSnapshotForTick(int tick, out HitboxSnapshot snapshot)
	{
		int num = tick - Tick;
		if (num > 0)
		{
			snapshot = _buffer[_head];
			InternalLogStreams.LogDebug?.Warn($"Tick {tick} is not in the Hitbox history, using closest instead: {snapshot.Tick}. Buffer length: {Length}, Buffer current tick: {Tick}");
		}
		else if (num < 1 - Length)
		{
			int num2 = ((_advanced < Length) ? 1 : ((_head + 1) % Length));
			snapshot = _buffer[num2];
			InternalLogStreams.LogDebug?.Warn($"Tick {tick} is not in the Hitbox history, using closest instead: {snapshot.Tick}. Buffer length: {Length}, Buffer current tick: {Tick}");
		}
		else
		{
			snapshot = _buffer[(_head + num + Length) % Length];
			Assert.Check(snapshot.Tick == tick, "The hitbox buffer seems to be missing the correct snapshot, make sure lag compensation is enabled in the network project config.");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void QueryBroadphase(Query query, HashSet<int> processedColliderIndices, out IHitboxColliderContainer container)
	{
		query.Tick = GetClosestTick(query);
		_broadphaseCandidates.Clear();
		GetClosestSnapshotForTick(query.Tick.Value, out var snapshot);
		snapshot.QueryBroadphase(query, _broadphaseCandidates);
		if ((query.Options & HitOptions.SubtickAccuracy) == HitOptions.SubtickAccuracy && query.TickTo.HasValue && query.Alpha.HasValue)
		{
			GetClosestSnapshotForTick(query.TickTo.Value, out var snapshot2);
			snapshot2.QueryBroadphase(query, _broadphaseCandidates);
			query.PreProcessingDelegate?.Invoke(query, _broadphaseCandidates, processedColliderIndices);
			HitboxSnapshot.ProcessBroadphaseRootCandidates(query, snapshot, _broadphaseCandidates, processedColliderIndices, snapshot2);
		}
		else
		{
			query.PreProcessingDelegate?.Invoke(query, _broadphaseCandidates, processedColliderIndices);
			HitboxSnapshot.ProcessBroadphaseRootCandidates(query, snapshot, _broadphaseCandidates, processedColliderIndices);
		}
		container = snapshot;
	}

	private int GetClosestTick(Query query)
	{
		if (!query.TickTo.HasValue || (query.Options & HitOptions.SubtickAccuracy) == HitOptions.SubtickAccuracy)
		{
			return query.Tick.Value;
		}
		return Mathf.RoundToInt(Mathf.Lerp(query.Tick.Value, query.TickTo.Value, query.Alpha.Value));
	}
}
