using UnityEngine;

namespace Oculus.Interaction;

public class RandomSampleConsensus<TModel>
{
	public delegate TModel GenerateModel(int index1, int index2);

	public delegate float EvaluateModelScore(TModel model, TModel[,] modelSet);

	private readonly TModel[,] _modelSet;

	private readonly int _exclusionZone;

	private readonly int _maxDataPoints;

	public RandomSampleConsensus(int maxDataPoints = 10, int exclusionZone = 2)
	{
		_maxDataPoints = maxDataPoints;
		_exclusionZone = exclusionZone;
		_modelSet = new TModel[maxDataPoints, maxDataPoints];
	}

	public TModel FindOptimalModel(GenerateModel modelGenerator, EvaluateModelScore modelScorer)
	{
		return FindOptimalModel(modelGenerator, modelScorer, _maxDataPoints);
	}

	public TModel FindOptimalModel(GenerateModel modelGenerator, EvaluateModelScore modelScorer, int dataPointsCount)
	{
		for (int i = 0; i < dataPointsCount; i++)
		{
			for (int j = i + 1; j < dataPointsCount; j++)
			{
				_modelSet[i, j] = modelGenerator((i + _exclusionZone) % dataPointsCount, (j + _exclusionZone) % dataPointsCount);
			}
		}
		TModel result = default(TModel);
		float num = float.PositiveInfinity;
		for (int k = 0; k < dataPointsCount; k++)
		{
			int num2 = Random.Range(0, dataPointsCount - 1);
			int num3 = Random.Range(num2 + 1, dataPointsCount);
			TModel val = _modelSet[num2, num3];
			float num4 = modelScorer(val, _modelSet);
			if (num4 < num)
			{
				result = val;
				num = num4;
			}
		}
		return result;
	}
}
