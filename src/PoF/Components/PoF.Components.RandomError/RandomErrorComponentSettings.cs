using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PoF.Components.RandomError
{
    public class RandomErrorComponentSettings
    {
        public double FailureRisk { get; set; } = 0.5;
        public double CompensationFailureRisk { get; set; } = 0;
        public bool SkipDelay { get; set; } = false;

        internal static RandomErrorComponentSettings GetSettings(string componentSettings)
        {
            if (string.IsNullOrEmpty(componentSettings))
            {
                return new RandomErrorComponentSettings();
            }
            try
            {
                return JsonConvert.DeserializeObject<RandomErrorComponentSettings>(componentSettings);
            }
            catch
            {
                return new RandomErrorComponentSettings();
            }
        }
    }
}
