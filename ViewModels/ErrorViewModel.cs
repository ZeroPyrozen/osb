using osb.Models;
using System;

namespace osb.ViewModels
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public BeatmapModel randomBeatmap { get; set; }
        public string baseURL { get; set; }
        public string ErrorMessage { get; set; }
    }
}
