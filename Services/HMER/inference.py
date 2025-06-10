import os
import cv2
import argparse
import numpy as np
import torch
import json
from tqdm import tqdm

from utils import load_config, load_checkpoint
from infer.Backbone import Backbone
from dataset import Words

parser = argparse.ArgumentParser(description='Spatial channel attention')
parser.add_argument('--config', default='config.yaml', type=str, help='配置文件路径')
parser.add_argument('--image_path', default='/home/yuanye/work/data/CROHME2014/14_off_image_test', type=str, help='测试image路径')
args = parser.parse_args()

if not args.config:
    print('请提供config yaml路径！')
    exit(-1)

"""加载config文件"""
params = load_config(args.config)

device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
params['device'] = device

words = Words(params['word_path'])
params['word_num'] = len(words)
params['struct_num'] = 7
params['words'] = words

model = Backbone(params)
model = model.to(device)

load_checkpoint(model, None, params['checkpoint'])

model.eval()

word_right, node_right, exp_right, length, cal_num = 0, 0, 0, 0, 0

def convert(nodeid, gtd_list):
    isparent = False
    child_list = []
    for i in range(len(gtd_list)):
        if gtd_list[i][2] == nodeid:
            isparent = True
            child_list.append([gtd_list[i][0],gtd_list[i][1],gtd_list[i][3]])
    if not isparent:
        return [gtd_list[nodeid][0]]
    else:
        if gtd_list[nodeid][0] == '\\frac':
            return_string = [gtd_list[nodeid][0]]
            for i in range(len(child_list)):
                if child_list[i][2] == 'Above':
                    return_string += ['{'] + convert(child_list[i][1], gtd_list) + ['}']
            for i in range(len(child_list)):
                if child_list[i][2] == 'Below':
                    return_string += ['{'] + convert(child_list[i][1], gtd_list) + ['}']
            for i in range(len(child_list)):
                if child_list[i][2] == 'Right':
                    return_string += convert(child_list[i][1], gtd_list)
            for i in range(len(child_list)):
                if child_list[i][2] not in ['Right','Above','Below']:
                    return_string += ['illegal']
        else:
            return_string = [gtd_list[nodeid][0]]
            for i in range(len(child_list)):
                if child_list[i][2] in ['l_sup']:
                    return_string += ['['] + convert(child_list[i][1], gtd_list) + [']']
            for i in range(len(child_list)):
                if child_list[i][2] == 'Inside':
                    return_string += ['{'] + convert(child_list[i][1], gtd_list) + ['}']
            for i in range(len(child_list)):
                if child_list[i][2] in ['Sub','Below']:
                    return_string += ['_','{'] + convert(child_list[i][1], gtd_list) + ['}']
            for i in range(len(child_list)):
                if child_list[i][2] in ['Sup','Above']:
                    return_string += ['^','{'] + convert(child_list[i][1], gtd_list) + ['}']
            for i in range(len(child_list)):
                if child_list[i][2] in ['Right']:
                    return_string += convert(child_list[i][1], gtd_list)
        return return_string
    

def resize_to_exact_pixels(width, height, target_pixels=80000):
    aspect_ratio = width / height
    
    new_height = np.sqrt(target_pixels / aspect_ratio)
    new_width = new_height * aspect_ratio
    
    new_height = int(round(new_height))
    new_width = int(round(new_width))
    
    new_height = max(1, new_height)
    new_width = max(1, new_width)
    
    return new_width, new_height

def process_image_to_binary(image, target_pixels=80000):
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    
    blur = cv2.GaussianBlur(gray, (5, 5), 0)
    
    _, threshold = cv2.threshold(blur, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)
    
    binary_height, binary_width = threshold.shape[:2]
    new_binary_width, new_binary_height = resize_to_exact_pixels(binary_width, binary_height, target_pixels)
    binary = cv2.resize(threshold, (new_binary_width, new_binary_height), interpolation=cv2.INTER_NEAREST)
    
    non_zero_pixels = cv2.findNonZero(binary)
    if non_zero_pixels is not None:
        x, y, w, h = cv2.boundingRect(non_zero_pixels)
        
        margin = 10
        x = max(0, x - margin)
        y = max(0, y - margin)
        w = min(binary.shape[1] - x, w + 2 * margin)
        h = min(binary.shape[0] - y, h + 2 * margin)
        
        binary_cropped = binary[y:y+h, x:x+w]
    else:
        binary_cropped = binary
    
    return binary_cropped, gray, binary


with torch.no_grad():
    img = cv2.imread(os.path.join(args.image_path))
    img = process_image_to_binary(img)[0]
    image = torch.Tensor(img) / 255
    image = image.unsqueeze(0).unsqueeze(0)

    image_mask = torch.ones(image.shape)
    image, image_mask = image.to(device), image_mask.to(device)

    prediction = model(image, image_mask)

    latex_list = convert(1, prediction)
    latex_string = ' '.join(latex_list)

    print(latex_string)














