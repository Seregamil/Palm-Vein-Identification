## Palm Vein Identification 

All work writed on **C#** with Jupyter Notebook *(.Net Interactive)*
Used *CASIA Multi-Spectral Palmprint Database*: http://biometrics.idealtest.org/dbDetailForUser.do?id=6

Sample preprocessing u can see in [jupyter notebook](net-interactive/image-preprocess-sample.ipynb) (.net interactive)  
This method very slowly, ~3h for 7200images  
New ROI extracting method u can see in this [jupyter notebook](net-interactive/roi-new-method.ipynb) (.net interactive), ~2min for 7200images

## Image processing time


Rotation angles: 0, 10, 20, 30, 40, 50, 60, 120, 130, 140, 150, 330

Elapsed preprocess time: **00:47:47.2919918**
**72** img per hand, **144** per human
Total output images: **14400**

Model accuracy: **51.70%**
Training time: **~7hours**


Rotation angles: 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 120, 125, 130, 135, 140, 145, 150, 330

Elapsed preprocess time: **00:47:47.2919918**
**126** img per hand, **252** per human

Total output images: **25200**
Model accuracy: **????**

Training time: **~????**


## Packages
	
1. [OpenCvSharp4.Windows](https://github.com/shimat/opencvsharp)
2. [ML.Net](https://github.com/dotnet/machinelearning)
3. [XPlot.Plotly](https://github.com/fslaborg/XPlot)
4. [MoreLinq](https://morelinq.github.io/)
5. [TensorFlow](https://storage.googleapis.com/tensorflow/windows/gpu/tensorflow_gpu-2.1.0-cp37-cp37m-win_amd64.whl)