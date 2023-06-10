import math
import numpy as np


def get_total_touch_movement(touch1_list, touch2_list, state_list):
    convert_format_and_units(touch1_list)
    convert_format_and_units(touch2_list)
    previous_coordinates1 = [0,0] 
    previous_coordinates2 = [0,0]
    total_movement = 0
    for i in range(0, len(state_list)):
        if state_list[i] == "Idle":
            previous_coordinates1 == [0,0]
            previous_coordinates2 == [0,0]
            continue

        #In the case of RotationX and RotationZ, consider the average of both fingers
        if state_list[i] == "RotationX" or state_list == "RotationZ":
            distance1 = 0
            distance2 = 0
            if isinstance(touch1_list[i], list):
                if previous_coordinates1 == [0,0]:
                    previous_coordinates1 = touch1_list[i]
                else:
                    #calculate distance between previous and current coordinates
                    distance1 = math.sqrt((touch1_list[i][0] - previous_coordinates1[0])**2 + (touch1_list[i][1] - previous_coordinates1[1])**2)
                    previous_coordinates1 = touch1_list[i]
                    #add distance to total movement

            if isinstance(touch2_list[i], list):
                if previous_coordinates2 == [0,0]:
                    previous_coordinates2 = touch2_list[i]
                else:
                    #calculate distance between previous and current coordinates
                    distance2 = math.sqrt((touch2_list[i][0] - previous_coordinates2[0])**2 + (touch2_list[i][1] - previous_coordinates2[1])**2)
                    previous_coordinates2 = touch2_list[i]
                    #add distance to total movement

            total_movement += (distance1 + distance2)/2
            continue


        if isinstance(touch1_list[i], list):
            if previous_coordinates1 == [0,0]:
                previous_coordinates1 = touch1_list[i]
            else:
                #calculate distance between previous and current coordinates
                distance = math.sqrt((touch1_list[i][0] - previous_coordinates1[0])**2 + (touch1_list[i][1] - previous_coordinates1[1])**2)
                previous_coordinates1 = touch1_list[i]
                #add distance to total movement
                total_movement += distance

        if isinstance(touch2_list[i], list):
            if previous_coordinates2 == [0,0]:
                previous_coordinates2 = touch2_list[i]
            else:
                #calculate distance between previous and current coordinates
                distance = math.sqrt((touch2_list[i][0] - previous_coordinates2[0])**2 + (touch2_list[i][1] - previous_coordinates2[1])**2)
                previous_coordinates2 = touch2_list[i]
                #add distance to total movement
                total_movement += distance
    
    return total_movement





def get_total_translation_touch_movement(touch1_list, touch2_list, state_list):
    convert_format_and_units(touch1_list)
    convert_format_and_units(touch2_list)
    previous_coordinates1 = [0,0] 
    previous_coordinates2 = [0,0]
    total_movement = 0
    for i in range(0, len(state_list)):
        if state_list[i] != "TranslationXZ" and state_list[i] != "TranslationY":
            previous_coordinates1 == [0,0]
            previous_coordinates2 == [0,0]
            continue
        if isinstance(touch1_list[i], list):
            if previous_coordinates1 == [0,0]:
                previous_coordinates1 = touch1_list[i]
            else:
                #calculate distance between previous and current coordinates
                distance = math.sqrt((touch1_list[i][0] - previous_coordinates1[0])**2 + (touch1_list[i][1] - previous_coordinates1[1])**2)
                previous_coordinates1 = touch1_list[i]
                #add distance to total movement
                total_movement += distance

        if isinstance(touch2_list[i], list):
            if previous_coordinates2 == [0,0]:
                previous_coordinates2 = touch2_list[i]
            else:
                #calculate distance between previous and current coordinates
                distance = math.sqrt((touch2_list[i][0] - previous_coordinates2[0])**2 + (touch2_list[i][1] - previous_coordinates2[1])**2)
                previous_coordinates2 = touch2_list[i]
                #add distance to total movement
                total_movement += distance
    
    return total_movement


def convert_format_and_units(touch_list):
    #Convert (x;y) format into a list [x, y]
    for i in range(0, len(touch_list)):
        if isinstance(touch_list[i], str):
            touch_list[i] = touch_list[i].split(";")
            #remove brackets
            touch_list[i][0] = touch_list[i][0][1:]
            touch_list[i][1] = touch_list[i][1][:-1]
            touch_list[i] = [float(touch_list[i][0]), float(touch_list[i][1])]

    #Convert coordinates from pixels to cm, frame measures 1920x1080 pixels and 0.71x0.40 m
    for i in range(0, len(touch_list)):
        if isinstance(touch_list[i], list):
            touch_list[i][0] = touch_list[i][0] * (0.71/1920)
            touch_list[i][1] = touch_list[i][1] * (0.40/1080)
    return touch_list


def convert_format_3D(positions_list):
    #Convert (x;y;z) format into a list [x, y, z]
    for i in range(0, len(positions_list)):
        positions_list[i] = positions_list[i].split(";")
        #remove brackets
        positions_list[i][0] = positions_list[i][0][1:]
        positions_list[i][1] = positions_list[i][1]
        positions_list[i][2] = positions_list[i][2][:-1]
        positions_list[i] = [float(positions_list[i][0]), float(positions_list[i][1]), float(positions_list[i][2])]

    return positions_list


def get_total_homer_movement(positions_list):
    convert_format_3D(positions_list)
    previous_coordinates = [0,0,0] 
    total_movement = 0
    for i in range(0, len(positions_list)):
        if previous_coordinates == [0,0,0]:
            previous_coordinates = positions_list[i]
        else:
            #calculate distance between previous and current coordinates
            distance = math.sqrt((positions_list[i][0] - previous_coordinates[0])**2 + (positions_list[i][1] - previous_coordinates[1])**2 + (positions_list[i][2] - previous_coordinates[2])**2)
            previous_coordinates = positions_list[i]
            #add distance to total movement
            total_movement += distance

    return total_movement