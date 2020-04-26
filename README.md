## Palm Vein Identification 

Development is in **C#** with **.net interactive**
Database *CASIA Multi-Spectral Palmprint Database*: http://biometrics.idealtest.org/dbDetailForUser.do?id=6

ROI extracting method: [jupyter notebook](net-interactive/roi-new-method.ipynb) (.net interactive)
On average, it took about **2** minutes to process **7200** images.

Due to the fact that the data set is rather poor for training, an attempt was made to artificially increase the amount of data.
To do this, the original image is rotated by a certain angle and the ROI is extracted, expanding the data set.

Source dataset: **6** images on one palm on one frequency.  
Dataset now: **1080** images on one palm on one frequency. 

Image rotation angles: **0, 2, 4, ... 358**  
Image processing time (1 hand - 940nm): **04:51:25.1516273**   

The total number of output images: **108000**

Model accuracy for **108000** images: **??.??%**  
Elapsed training time for **108000** images: **??d ??h ??m**

Model accuracy for **12600** images: **64,20%**  
Elapsed training time for **12600** images: **11h 20m**

## Packages
	
1. [OpenCvSharp4.Windows](https://github.com/shimat/opencvsharp)
2. [ML.Net](https://github.com/dotnet/machinelearning)
3. [TensorFlow](https://storage.googleapis.com/tensorflow/windows/gpu/tensorflow_gpu-2.1.0-cp37-cp37m-win_amd64.whl)
