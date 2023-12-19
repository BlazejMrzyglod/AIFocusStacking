using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	public interface IFocusStackingService
	{
		ServiceResult RunFocusStacking(IEnumerable<string> photos, bool alignment, bool gauss, int laplaceSize, int gaussSize, bool takeAll, int masSize);
	}
}
