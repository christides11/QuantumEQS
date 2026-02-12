using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine.Serialization;

namespace Quantum.EQS
{
	[System.Serializable]
	public unsafe class EnvQueryGeneratorPointGrid : EnvQueryGenerator
	{
		public EnvQueryContext generateAround = EnvQueryContext.Querier;
		public FP radius = 4;
		public FP spaceBetween = 1;
		public FP projectionUpDown = 4;
		public FP preProjectionVerticalOffset = 0;
		[FormerlySerializedAs("portProjectionVerticalOffset")] public FP postProjectionVerticalOffset = 0;
		
		public override List<EnvQueryItem> GenerateItems(Frame frame, EnvQuery* envQuery, int numTests, Transform3D centerOfItems)
		{
			var origin = GetPosition(frame, generateAround, envQuery);
			origin += FPVector3.Up * preProjectionVerticalOffset;
			
			List<EnvQueryItem> items = new List<EnvQueryItem>();

			FPVector3 position = default;
			items.Add(new EnvQueryItem(numTests, position, origin));
			int numOfSteps = (int)FPMath.Ceiling(radius / spaceBetween);

			// First quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = xi * spaceBetween + spaceBetween / (FP)2;
					position.Y = 0;
					position.Z = zi * spaceBetween + spaceBetween / (FP)2;
					items.Add(new EnvQueryItem(numTests, position, origin));
				}
			}

			// Second quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = -(xi * spaceBetween + spaceBetween / (FP)2);
					position.Y = 0;
					position.Z = zi * spaceBetween + spaceBetween / (FP)2;
					items.Add(new EnvQueryItem(numTests, position, origin));
				}
			}

			// Third quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = -(xi * spaceBetween + spaceBetween / (FP)2);
					position.Y = 0;
					position.Z = -(zi * spaceBetween + spaceBetween / (FP)2);
					items.Add(new EnvQueryItem(numTests, position, origin));
				}
			}

			// Fourth quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = xi * spaceBetween + spaceBetween / (FP)2;
					position.Y = 0;
					position.Z = -(zi * spaceBetween + spaceBetween / (FP)2);
					items.Add(new EnvQueryItem(numTests, position, origin));
				}
			}

			foreach (var i in items)
			{
				switch (projectionType)
				{
					case ProjectionType.Navigation:
						i.UpdateNavMeshProjection(frame, projectionUpDown, postProjectionVerticalOffset);
						break;
					case ProjectionType.Trace:
						i.UpdateTraceProjection(frame, projectionUpDown, postProjectionVerticalOffset, projectionLayerMask);
						break;
				}
			}

			return items;
		}

		public override int CreateBroadphaseQueryForItems(Frame frame, EnvQueryCached* envQuery, int numTests, Transform3D centerOfItems)
		{
			var envQueryItemsList = frame.ResolveList(envQuery->envQueryItems);
			var broadphaseIndexes = frame.ResolveList(envQuery->broadphaseQueries);

			var startBroadphaseCount = broadphaseIndexes.Count;
			
			var envQueryItems = new List<EnvQueryItemCached>(envQueryItemsList);

			FPVector3 position = default;
			// Origin
			var origin = GetPosition(frame, generateAround, envQuery);
			origin += FPVector3.Up * preProjectionVerticalOffset;

			envQueryItems.Add(new EnvQueryItemCached(origin+position));
			int numOfSteps = (int)FPMath.Ceiling(radius / spaceBetween);
			
			// First quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = xi * spaceBetween + spaceBetween / (FP)2;
					position.Y = 0;
					position.Z = zi * spaceBetween + spaceBetween / (FP)2;
					envQueryItems.Add(new EnvQueryItemCached(origin+position));
				}
			}

			// Second quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = -(xi * spaceBetween + spaceBetween / (FP)2);
					position.Y = 0;
					position.Z = zi * spaceBetween + spaceBetween / (FP)2;
					envQueryItems.Add(new EnvQueryItemCached(origin+position));
				}
			}

			// Third quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = -(xi * spaceBetween + spaceBetween / (FP)2);
					position.Y = 0;
					position.Z = -(zi * spaceBetween + spaceBetween / (FP)2);
					envQueryItems.Add(new EnvQueryItemCached(origin+position));
				}
			}

			// Fourth quadrant
			for (int xi = 0; xi < numOfSteps; xi++)
			{
				for (int zi = 0; zi < numOfSteps; zi++)
				{
					position.X = xi * spaceBetween + spaceBetween / (FP)2;
					position.Y = 0;
					position.Z = -(zi * spaceBetween + spaceBetween / (FP)2);
					envQueryItems.Add(new EnvQueryItemCached(origin+position));
				}
			}
			
			foreach (var i in envQueryItems)
			{
				switch (projectionType)
				{
					case ProjectionType.Trace:
						i.CalculateTraceProjection(frame, envQuery, projectionUpDown, projectionLayerMask);
						break;
				}
				envQueryItemsList.Add(i);
			}

			return broadphaseIndexes.Count;
		}

		public override void GenerateItemsFromBroadphaseQueries(Frame frame, EnvQueryCached* envQuery, int numTests, Transform3D centerOfItems)
		{
			var envQueryItemsList = frame.ResolveList(envQuery->envQueryItems);
			var broadphaseIndexes = frame.ResolveList(envQuery->broadphaseQueries);
			var ind = 0;

			for (int index = 0; index < envQueryItemsList.Count; index++)
			{
				switch (projectionType)
				{
					case ProjectionType.Trace:
						envQueryItemsList.GetPointer(index)->FinalizeTraceProjection(frame, envQuery, postProjectionVerticalOffset, ref ind);
						break;
				}
			}
		}
	}
}