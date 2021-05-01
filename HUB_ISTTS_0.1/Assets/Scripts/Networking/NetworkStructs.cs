using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Drawing;
using System;

public class NetworkStructs
{
    public enum MessageTypes : byte
    {
        MESSAGE_LIST_START = 0,

        CNN,
        DC,
        MESSAGE,
        ASKNAME,
        NAMEIS,
        ROT,
        MOVE,

        MESSAGE_LIST_END
    }

    //General Int Data
    public struct IntData
    {
        public int data;

        public IntData(int var)
        {
            data = var;
        }
    }

    //General String Data
    public struct StringData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string str;

        public StringData(string str)
        {
            this.str = str;
        }
    }

    public struct NameRequestData
    {
        public int id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string str;

        public NameRequestData(int id, string str)
        {
            this.id = id;
            this.str = str;
        }
    }

    //Player Rotational Data
    public struct RotationData
    {
        public int id;

        public float xRot;
        public float yRot;

        public RotationData(int id, float x, float y)
        {
            this.id = id;

            xRot = x;
            yRot = y;
        }
    }

    //Player Velocity and Position Data
    public struct PositionVelocityData
    {
        public int id;

        public float xPos;
        public float yPos;
        public float zPos;

        public float xVel;
        public float yVel;
        public float zVel;

        public PositionVelocityData(int id, float x, float y, float z, float dx, float dy, float dz)
        {
            this.id = id;

            xPos = x;
            yPos = y;
            zPos = z;

            xVel = x;
            yVel = y;
            zVel = z;
        }
    }

    /*
    //Message Data
    public struct MessageData
    {
        MessageTypes type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        string message;

        public MessageData(string msg)
        {
            type = MessageTypes.MESSAGE;

            message = msg;
        }
    }*/

    public static byte[] AddTag(MessageTypes type, byte[] arr)
    {
        int len = arr.Length;
        byte[] newArr = new byte[len+1];
        newArr[0] = (byte)type;
        for (int i = 1; i < newArr.Length; ++i)
        {
            newArr[i] = arr[i - 1];
        }

        return newArr;
    }

    //Write struct T to byte array
    public static byte[] getBytes<T>(T str) where T : struct
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    //Read byte array to struct T
    public static T fromBytes<T>(byte[] arr) where T : struct
    {
        T str = new T();

        int size = Marshal.SizeOf(arr);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (T)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }

}
