import csv
import os
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib

import convert 


#Open .csv file with format and put it in a pandas dataframe
def open_csv(filename):
    df = pd.read_csv(filename)
    return df


#Open every folder in the /data folder and put the data in a pandas dataframes
def open_all_csv(technique):
    df_list = []
    for folder in os.listdir("data"):
        if folder != ".DS_Store":
            suffix = ""
            if technique == 0:
                suffix = "/touch.csv"
            elif technique == 1:
                suffix = "/touch_frames.csv"
            elif technique == 2:
                suffix = "/homer.csv"
            elif technique == 3:
                suffix = "/homer_frames.csv"
            df = open_csv("data/" + folder + suffix)
            df_list.append(df)
    return df_list



df_list_touch = open_all_csv(0)
df_list_touch_frames = open_all_csv(1)
df_list_homer = open_all_csv(2)
df_list_homer_frames = open_all_csv(3)


# Define the categories you want to extract
categories = ['Time', 'TimeSpentIdle', 'ActiveTime', 'TotalObjectTranslation', 'TotalMovement', 'TotalTranslationMovement']

# Create a new Excel file
writer = pd.ExcelWriter('Output.xlsx')

# Iterate over each category
for category in categories:
    for task in range(0, 8):
        #create new dataframe with Task, SIT6 and Scaled HOMER 
        merged_df = pd.DataFrame(columns=['Participant', 'SIT6', 'Scaled HOMER'])
        #write the participant number in the first column, which is the size of df_list_touch
        merged_df['Participant'] = range(1, len(df_list_touch) + 1)


        if category == 'TotalObjectTranslation':
            for i in range(0, len(df_list_touch)):
                #Get "TotalTranslationXZ" and "TotalTranslationY"
                total_translation_xz = df_list_touch[i]["TotalTranslationXZ"][df_list_touch[i]["Task"] == task + 1].sum()
                total_translation_y = df_list_touch[i]["TotalTranslationY"][df_list_touch[i]["Task"] == task + 1].sum()

                #Calculate the total translation
                total_translation = total_translation_xz + total_translation_y

                #Put the total translation in the dataframe
                merged_df.loc[i, 'SIT6'] = total_translation
            
            for i in range(0, len(df_list_homer)):
                #Get "TotalTranslation" and put it in the dataframe
                merged_df.loc[i, 'Scaled HOMER'] = df_list_homer[i]["TotalTranslation"][df_list_homer[i]["Task"] == task + 1].sum()  
        elif category == 'TotalMovement':
            for i in range(0, len(df_list_touch_frames)):
                #Get "TouchPosition1" and "TouchPosition2" lines in this task and convert them to a list
                touch_positions_1 = df_list_touch_frames[i]["TouchPosition1"][df_list_touch_frames[i]["Task"] == task + 1].tolist()
                touch_positions_2 = df_list_touch_frames[i]["TouchPosition2"][df_list_touch_frames[i]["Task"] == task + 1].tolist()

                #Get "State" line in this task and convert it to a list
                state = df_list_touch_frames[i]["State"][df_list_touch_frames[i]["Task"] == task + 1].tolist()

                merged_df.loc[i, 'SIT6'] = convert.get_total_touch_movement(touch_positions_1, touch_positions_2, state)

            for i in range(0, len(df_list_homer_frames)):
                #Get "ControllerPosition" line in this task and convert it to a list
                controller_positions = df_list_homer_frames[i]["ControllerPosition"][df_list_homer_frames[i]["Task"] == task + 1].tolist()

                merged_df.loc[i, 'Scaled HOMER'] = convert.get_total_homer_movement(controller_positions)

        elif category == 'TotalTranslationMovement':
            for i in range(0, len(df_list_touch_frames)):
                #Get "TouchPosition1" and "TouchPosition2" lines in this task and convert them to a list
                touch_positions_1 = df_list_touch_frames[i]["TouchPosition1"][df_list_touch_frames[i]["Task"] == task + 1].tolist()
                touch_positions_2 = df_list_touch_frames[i]["TouchPosition2"][df_list_touch_frames[i]["Task"] == task + 1].tolist()

                #Get "State" line in this task and convert it to a list
                state = df_list_touch_frames[i]["State"][df_list_touch_frames[i]["Task"] == task + 1].tolist()

                merged_df.loc[i, 'SIT6'] = convert.get_total_translation_touch_movement(touch_positions_1, touch_positions_2, state)

            for i in range(0, len(df_list_homer_frames)):
                #Get "ControllerPosition" line in this task and convert it to a list
                controller_positions = df_list_homer_frames[i]["ControllerPosition"][df_list_homer_frames[i]["Task"] == task + 1].tolist()

                merged_df.loc[i, 'Scaled HOMER'] = convert.get_total_homer_movement(controller_positions)
        elif category == 'ActiveTime':
            for i in range(0, len(df_list_touch)):
                #Get the time value of the task and subtract the time spent idle from it
                merged_df.loc[i, 'SIT6'] = df_list_touch[i]['Time'][df_list_touch[i]["Task"] == task + 1].sum() - df_list_touch[i]['TimeSpentIdle'][df_list_touch[i]["Task"] == task + 1].sum()

            for i in range(0, len(df_list_homer)):
                #Get the time value of the task and subtract the time spent idle from it
                merged_df.loc[i, 'Scaled HOMER'] = df_list_homer[i]['Time'][df_list_homer[i]["Task"] == task + 1].sum() - df_list_homer[i]['TimeSpentIdle'][df_list_homer[i]["Task"] == task + 1].sum()
        else:
            # Iterate over each participant
            for i in range(0, len(df_list_touch)):
                #Get the cell value of the category and task and put it in the dataframe
                merged_df.loc[i, 'SIT6'] = df_list_touch[i][category][df_list_touch[i]["Task"] == task + 1].sum()

            for i in range(0, len(df_list_homer)):
                merged_df.loc[i, 'Scaled HOMER'] = df_list_homer[i][category][df_list_homer[i]["Task"] == task + 1].sum()

    

        # Write the dataframe to the Excel file
        merged_df.to_excel(writer, sheet_name=category + " - T" + str(task + 1), index=False)
    



# Save and close the Excel file
writer.save()


