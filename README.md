# MemRecord
Pretty straight forward to use.

To save yourself some time, I recommend putting the name of the process you wanna record in the config file so you don't have to select the process each time you launch.

1. Use MemRecord either targeted for x64 or x86, depending on the process you're using it for.
2. Enter the address of the memory area you wanna record.
3. Enter how many bytes you want to record from that address 
4. Record

Once you stop recording it will generate a table of offsets that had their values change during the time you were recording. 

**The data it records is split up into 4 byte chunks (8 bytes if x64)**

![auqMQtN](https://user-images.githubusercontent.com/84111816/152670079-75f3036a-266d-4aa7-899f-452891979750.jpg)

Feel free to contribute, there's always something to be added or improved.
