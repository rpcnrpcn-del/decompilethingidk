import glob
import os
for file in glob.glob("*.prefab"):
    is_meta = False
    if not file.startswith("["):
        if file.endswith(".meta"):
            is_meta = True

        file_name = file
        file_name = file.replace(".prefab", "")
        file_name = file.replace(".meta", "")

        real_path = list(file_name)
        new_path = list(file_name)

        new_path[0] = "["
        new_path[len(new_path)-1] = "]"
        
        if is_meta:
            real_path = "".join(real_path) + ".prefab" + ".meta"
            new_path = "".join(new_path) + ".prefab" + ".meta"
        else:
            real_path = "".join(real_path) + ".prefab"
            new_path = "".join(new_path) + ".prefab"

        real_path = os.path.realpath(real_path)
        new_path = os.path.realpath(new_path)

        #os.rename(real_path, new_path)
        
        print("Renaming: " + real_path + " to: " + new_path)