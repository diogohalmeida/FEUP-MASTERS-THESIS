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

#Touch dataframe has the format Task,Time,DistanceMismatch,RotationMismatchX,RotationMismatchY,RotationMismatchZ,TimeSpentIdle,TimeSpentChecking,TimeSpentTranslationXZ,TimeSpentTranslationY,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslationXZ,TotalTranslationY,TotalRotationX,TotalRotationY,TotalRotationZ
#Plot the all the times spent in task 1
def plot_task(df_list, task):
    time_spent = []
    for df in df_list:
        time_spent.append(df[df["Task"] == task]["Time"].sum())
    plt.hist(time_spent, bins=10)
    plt.title("Time spent in task " + str(task))
    plt.xlabel("Time spent (s)")
    plt.ylabel("Number of participants")
    plt.show()	


plot_task(df_list_touch, 1)
plot_task(df_list_homer, 1)

plot_task(df_list_touch, 2)
plot_task(df_list_homer, 2)

plot_task(df_list_touch, 3)
plot_task(df_list_homer, 3)

plot_task(df_list_touch, 4)
plot_task(df_list_homer, 4)

plot_task(df_list_touch, 5)
plot_task(df_list_homer, 5)

plot_task(df_list_touch, 6)
plot_task(df_list_homer, 6)

plot_task(df_list_touch, 7)
plot_task(df_list_homer, 7)

plot_task(df_list_touch, 8)
plot_task(df_list_homer, 8)