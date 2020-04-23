from __future__ import print_function
import sys, os, json, math
import numpy, scipy
import numpy as np

import keras
from keras import *
import tensorflow as tf
import keras.backend as K
from keras.preprocessing  import utils
from keras.preprocessing.image import *
from keras_preprocessing import *
from keras_tqdm import *
from keras_applications import *
from keras_contrib import *
from keras.models import *
from keras.datasets import *
from keras.layers import *
from keras.preprocessing import *
from keras.optimizers import  *
from keras.losses import  *
from keras.metrics import  *
from keras.callbacks import  *
from keras.activations import  *
from keras.regularizers import  *
from keras.layers.advanced_activations import PReLU

print(keras.__version__)
print(tf.__version__)

img_rows, img_cols, img_ch = 227, 227, 3
batch_size = 32
nepochs = 50
dir_tr = 'Model/l/'
dir_val = 'Model/l/'

train_datagen = ImageDataGenerator(rescale=1./255)
test_datagen = ImageDataGenerator(rescale=1./255)

train_generator = train_datagen.flow_from_directory(
    directory=dir_tr, target_size=(img_rows, img_cols))
validation_generator = test_datagen.flow_from_directory(
    directory=dir_tr, target_size=(img_rows, img_cols))

input_img = Input(shape=(img_rows, img_cols, img_ch))

classifier = Sequential()

# 1st Convolutional Layer
classifier.add(Conv2D(filters=96, input_shape=(img_rows, img_cols, img_ch),
                      kernel_size=(11, 11), strides=(4, 4), padding="valid", activation="relu"))

# Max Pooling
classifier.add(MaxPool2D(pool_size=(3, 3), strides=(2, 2), padding="valid"))

# 2nd Convolutional Layer
classifier.add(Conv2D(filters=256, kernel_size=(5, 5),
                      strides=(1, 1), padding="same", activation="relu"))

# Max Pooling
classifier.add(MaxPool2D(pool_size=(3, 3), strides=(2, 2), padding="valid"))

# 3rd Convolutional Layer
classifier.add(Conv2D(filters=384, kernel_size=(3, 3),
                      strides=(1, 1), padding="same", activation="relu"))

# 4th Convolutional Layer
classifier.add(Conv2D(filters=384, kernel_size=(3, 3),
                      strides=(1, 1), padding="same", activation="relu"))

# 5th Convolutional Layer
classifier.add(Conv2D(filters=256, kernel_size=(3, 3),
                      strides=(1, 1), padding="same", activation="relu"))

# Max Pooling
classifier.add(MaxPool2D(pool_size=(3, 3), strides=(2, 2), padding="valid"))

# Passing it to a Fully Connected layer
classifier.add(Flatten())
# 1st Fully Connected Layer
classifier.add(Dense(units=4096, activation="relu"))

# 2nd Fully Connected Layer
classifier.add(Dense(4096, activation="relu"))

# Output Layer
classifier.add(Dense(100, activation="softmax"))  # As we have two classes
classifier.summary()

opt = Adam(lr=0.0008)
classifier.compile(
    optimizer=opt, loss=keras.losses.categorical_crossentropy, metrics=['accuracy'])

history = classifier.fit_generator(
    train_generator,
    steps_per_epoch=((train_generator.samples))//batch_size,
    epochs=nepochs, verbose=1,
    validation_data=validation_generator,
    validation_steps=((validation_generator.samples)) // batch_size)
