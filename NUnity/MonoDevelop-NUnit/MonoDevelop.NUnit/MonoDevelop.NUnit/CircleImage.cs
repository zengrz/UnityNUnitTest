using Gdk;
using MonoDevelop.Core;
using System;
namespace MonoDevelop.NUnit
{
	internal static class CircleImage
	{
		internal static Pixbuf Running;
		internal static Pixbuf Failure;
		internal static Pixbuf None;
		internal static Pixbuf NotRun;
		internal static Pixbuf Success;
		internal static Pixbuf SuccessAndFailure;
		internal static Pixbuf Loading;
		static CircleImage()
		{
			try
			{
				CircleImage.Running = Pixbuf.LoadFromResource("NUnit.Running.png");
				CircleImage.Failure = Pixbuf.LoadFromResource("NUnit.Failed.png");
				CircleImage.None = Pixbuf.LoadFromResource("NUnit.None.png");
				CircleImage.NotRun = Pixbuf.LoadFromResource("NUnit.NotRun.png");
				CircleImage.Success = Pixbuf.LoadFromResource("NUnit.Success.png");
				CircleImage.SuccessAndFailure = Pixbuf.LoadFromResource("NUnit.SuccessAndFailed.png");
				CircleImage.Loading = Pixbuf.LoadFromResource("NUnit.Loading.png");
			}
			catch (System.Exception e)
			{
				LoggingService.LogError("Error while loading icons.", e);
			}
		}
	}
}
