import glob
import os
for file in glob.glob("*.prefab"):
    if not file.startswith("["):

        file_name = file

        file_name = file.removesuffix(".prefab")

        real_path = list(file_name)
        new_path = list(file_name)

        new_path[0] = "["
        new_path[len(new_path)-1] = "]"
        
        real_path = "".join(real_path) + ".prefab"
        new_path = "".join(new_path) + ".prefab"
        
        print("Renaming: " + real_path + " to: " + new_path + "\n")

        real_path = os.path.realpath(real_path)
        new_path = os.path.realpath(new_path)

        os.rename(real_path, new_path)

for file in glob.glob("*.meta"):
    if not file.startswith("[") and ".prefab" in file:

        file_name = file

        file_name = file.removesuffix(".meta").removesuffix(".prefab")

        real_path = list(file_name)
        new_path = list(file_name)

        new_path[0] = "["
        new_path[len(new_path)-1] = "]"
        
        real_path = "".join(real_path) + ".prefab.meta"
        new_path = "".join(new_path) + ".prefab.meta"
        
        print("Renaming: " + real_path + " to: " + new_path + "\n")

        real_path = os.path.realpath(real_path)
        new_path = os.path.realpath(new_path)

        os.rename(real_path, new_path)