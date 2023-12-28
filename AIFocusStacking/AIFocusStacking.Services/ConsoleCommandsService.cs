using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
    public class ConsoleCommandsService : IConsoleCommandsService
    {
        protected IPhotoRepositoryService _photoRepository;
        protected string outputDirectory;

        public ConsoleCommandsService(IPhotoRepositoryService photoRepository)
        {
            _photoRepository = photoRepository;
            outputDirectory = "outputImages";
        }
        public ServiceResult RunModel(string method)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                IEnumerable<string> photos = _photoRepository.GetAll();
                ProcessStartInfo start = new ProcessStartInfo();
                string script = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\demo.py";
                string configFile;
                string weights;
                string confidence;

				if (method == "2")
                {
                    configFile = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\configs\\Misc\\cascade_mask_rcnn_X_152_32x8d_FPN_IN5k_gn_dconv.yaml";//\\COCO-InstanceSegmentation\\mask_rcnn_X_101_32x8d_FPN_3x.yaml";
                    weights = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\model_0039999_e76410.pkl";// model_final_2d9806.pkl";
                    confidence = "0.5";
				}
                else 
                {
                    configFile = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\configs\\Misc\\panoptic_fpn_R_101_dconv_cascade_gn_3x.yaml";//\\COCO-PanopticSegmentation\\panoptic_fpn_R_101_3x.yaml";
                    weights = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\model_final_be35db.pkl";// model_final_cafdb1.pkl";
                    confidence = "1";
				}

				Directory.CreateDirectory(outputDirectory);
                string options = "MODEL.DEVICE cpu";
                

				start.FileName = "CMD.exe";
                start.Arguments = $"/C python {script} --config-file {configFile} --input {string.Join(" ", photos)} --output {outputDirectory} --confidence-threshold {confidence} --opts {options} MODEL.WEIGHTS {weights}";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string consoleResult = reader.ReadToEnd();
                        Console.Write(consoleResult);
                    }
                }
                //_photoRepository.DeleteMultiple(photos.ToArray());
                result.Result = ServiceResultStatus.Succes;
            }
            catch (Exception e)
            {
                result.Result = ServiceResultStatus.Error;
                result.Messages.Add(e.Message);
            }

            return result;
        }

        public ServiceResult ClearOutputDirectory()
        {
            ServiceResult result = new ServiceResult();
            try
            {
                Directory.Delete(outputDirectory, true);
                result.Result = ServiceResultStatus.Succes;
            }
            catch (Exception e)
            {
                result.Result = ServiceResultStatus.Error;
                result.Messages.Add(e.Message);
            }

            return result;
        }
    }
}
