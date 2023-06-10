import csv
import os
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib


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
categories = ['Time', 'DistanceMismatch', 'RotationMismatchX', 'RotationMismatchY', 'RotationMismatchZ']

# Create a new Excel file
writer = pd.ExcelWriter('Output.xlsx')

# Iterate over each category
for category in categories:
    for task in range(0, 8):
        #create new dataframe with Task, SIT6 and Scaled HOMER 
        merged_df = pd.DataFrame(columns=['Participant', 'SIT6', 'Scaled HOMER'])
        #write the participant number in the first column, which is the size of df_list_touch
        merged_df['Participant'] = range(1, len(df_list_touch) + 1)

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




#Touch dataframe has the format Task,Time,DistanceMismatch,RotationMismatchX,RotationMismatchY,RotationMismatchZ,TimeSpentIdle,TimeSpentChecking,TimeSpentTranslationXZ,TimeSpentTranslationY,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslationXZ,TotalTranslationY,TotalRotationX,TotalRotationY,TotalRotationZ
# #Plot the all the times spent in task 1
# def plot_task(df_list, task):
#     time_spent = []
#     for df in df_list:
#         time_spent.append(df[df["Task"] == task]["Time"].sum())
#     plt.hist(time_spent, bins=10)
#     plt.title("Time spent in task " + str(task))
#     plt.xlabel("Time spent (s)")
#     plt.ylabel("Number of participants")
#     plt.show()	


# plot_task(df_list_touch, 1)
# plot_task(df_list_homer, 1)

# plot_task(df_list_touch, 2)
# plot_task(df_list_homer, 2)

# plot_task(df_list_touch, 3)
# plot_task(df_list_homer, 3)

# plot_task(df_list_touch, 4)
# plot_task(df_list_homer, 4)

# plot_task(df_list_touch, 5)
# plot_task(df_list_homer, 5)

# plot_task(df_list_touch, 6)
# plot_task(df_list_homer, 6)

# plot_task(df_list_touch, 7)
# plot_task(df_list_homer, 7)

# plot_task(df_list_touch, 8)
# plot_task(df_list_homer, 8)
