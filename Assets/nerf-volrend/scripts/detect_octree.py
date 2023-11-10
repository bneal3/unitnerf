import argparse
import os
from PIL import Image

from ultralytics import YOLO
import cv2

# FLOW: Argument parsing setup
parser = argparse.ArgumentParser(description='Produce bounding box coordinates in image')
parser.add_argument('-model', '--modelpath', help='File path of the Yolo model to use')
parser.add_argument('-image', '--imagepath', help='File path of the image file to use')
parser.add_argument('-output', '--outputpath', help='File path of the output file')
args = parser.parse_args()

# FLOW: Instantiate model
model_path = os.path.abspath(args.modelpath)
model = YOLO(model_path)

# FLOW: Instantiate image
image_path = os.path.abspath(args.imagepath)
image = Image.open(image_path)

# FLOW: Get results
results = model.predict(image, conf=0.5)

# FLOW: Open file for write
file = open(args.outputpath, "w")

# FLOW: Write bounding box results to file
for result in results:
    for box in result.boxes:
        file.write(str(box.xywh[0][0].item()) + ' ' + str(box.xywh[0][1].item()) + ' ' + str(box.xywh[0][2].item()) + ' ' + str(box.xywh[0][3].item()) + ' ' + '\n')

# FLOW: Close file
file.close()