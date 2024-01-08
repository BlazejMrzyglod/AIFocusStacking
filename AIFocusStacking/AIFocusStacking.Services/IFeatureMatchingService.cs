using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	//Interfejs serwisu odpowiedzialnego za dobieranie wykrytych obiektów w pary
	public interface IFeatureMatchingService
	{
		int GetAmountOfMatches(Mat img1, Mat img2);
	}
}
