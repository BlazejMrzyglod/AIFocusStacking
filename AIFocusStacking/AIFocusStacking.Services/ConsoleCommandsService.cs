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
        public ServiceResult RunModel()
        {
            ServiceResult result = new ServiceResult();
            try
            {
                IEnumerable<string> photos = _photoRepository.GetAll();
                ProcessStartInfo start = new ProcessStartInfo();
                string script = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\demo.py";
                string configFile = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\projects\\PointRend\\configs\\InstanceSegmentation\\pointrend_rcnn_X_101_32x8d_FPN_3x_coco.yaml";
                Directory.CreateDirectory(outputDirectory);
                string options = "MODEL.DEVICE cpu";
                string weights = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\model_final_ba17b9.pkl";
                start.FileName = "CMD.exe";
                start.Arguments = $"/C python {script} --config-file {configFile} --input {string.Join(" ", photos)} --output {outputDirectory} --opts {options} MODEL.WEIGHTS {weights}";
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
                _photoRepository.DeleteMultiple(photos.ToArray());
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
