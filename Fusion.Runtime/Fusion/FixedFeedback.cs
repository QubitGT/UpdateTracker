#define DEBUG
namespace Fusion;

internal class FixedFeedback : IFeedbackController
{
	private readonly double _outputMin;

	private readonly double _outputMax;

	private readonly double _deadzoneMin;

	private readonly double _deadzoneMax;

	private double _output;

	public FixedFeedback(double outputMin, double outputMax, double deadzoneMin, double deadzoneMax)
	{
		Assert.Check(!double.IsNaN(outputMin));
		Assert.Check(!double.IsNaN(outputMax));
		Assert.Check(!double.IsInfinity(outputMin));
		Assert.Check(!double.IsInfinity(outputMax));
		Assert.Check(outputMin <= outputMax);
		_outputMin = outputMin;
		_outputMax = outputMax;
		_deadzoneMin = deadzoneMin;
		_deadzoneMax = deadzoneMax;
		_output = 0.0;
	}

	double IFeedbackController.Output()
	{
		return _output;
	}

	void IFeedbackController.Update(double sample, double target, double dt)
	{
		Assert.Check(!double.IsNaN(sample));
		Assert.Check(!double.IsInfinity(sample));
		Assert.Check(!double.IsNaN(target));
		Assert.Check(!double.IsInfinity(target));
		Assert.Check(!double.IsNaN(dt));
		Assert.Check(!double.IsInfinity(dt));
		double num = target - sample;
		if (num > _deadzoneMax)
		{
			_output = _outputMax;
		}
		else if (num < _deadzoneMin)
		{
			_output = _outputMin;
		}
		else
		{
			_output = 0.0;
		}
	}

	void IFeedbackController.Reset()
	{
		_output = 0.0;
	}

	void IFeedbackController.ResetOutput()
	{
		_output = 0.0;
	}
}
