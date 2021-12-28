﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent NeckTingle_mrse = new ManualResetEvent(false);
        private static ManualResetEvent TelekinesisR_mrse = new ManualResetEvent(false);
        private static ManualResetEvent TelekinesisL_mrse = new ManualResetEvent(false);
        public Dictionary<String, FileInfo> FeedbackMap = new Dictionary<String, FileInfo>();


        public void HeartBeatFunc()
        {
            while (true)
            {
                HeartBeat_mrse.WaitOne();
                bHaptics.SubmitRegistered("HeartBeat");
                Thread.Sleep(1000);
            }
        }

        public void NeckTingleFunc()
        {
            while (true)
            {
                NeckTingle_mrse.WaitOne();
                bHaptics.SubmitRegistered("NeckTingleShort");
                Thread.Sleep(2050);
            }
        }

        public void TelekinesisRFunc()
        {
            while (true)
            {
                TelekinesisR_mrse.WaitOne();
                bHaptics.SubmitRegistered("Telekinesis_R");
                Thread.Sleep(2050);
            }
        }
        public void TelekinesisLFunc()
        {
            while (true)
            {
                TelekinesisL_mrse.WaitOne();
                bHaptics.SubmitRegistered("Telekinesis_L");
                Thread.Sleep(2050);
            }
        }

        public TactsuitVR()
        {
            LOG("Initializing suit");
            if (!bHaptics.WasError)
            {
                suitDisabled = false;
            }
            RegisterAllTactFiles();
            LOG("Starting HeartBeat and NeckTingle thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread NeckTingleThread = new Thread(NeckTingleFunc);
            NeckTingleThread.Start();
            Thread TeleRThread = new Thread(TelekinesisRFunc);
            TeleRThread.Start();
            Thread TeleLThread = new Thread(TelekinesisLFunc);
            TeleLThread.Start();
        }

        public void LOG(string logStr)
        {
            MelonLogger.Msg(logStr);
        }



        void RegisterAllTactFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    bHaptics.RegisterFeedbackFromTactFile(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
            }
            systemInitialized = true;
            //PlaybackHaptics("HeartBeat");
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                if ((intensity != 1.0f)|(duration != 1.0f))
                {
                    bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(intensity, duration);
                    //float locationAngle = 0.0f;
                    //float locationHeight = 0.0f;
                    //bHaptics.RotationOption rotationOption = new bHaptics.RotationOption(locationAngle, locationHeight);
                    bHaptics.SubmitRegistered(key, key, scaleOption);
                }
                
                // LOG("Playing back: " + key);
                bHaptics.SubmitRegistered(key);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void GunRecoil(bool isRightHand, float intensity = 1.0f)
        {
            float duration = 1.0f;
            var scaleOption = new bHaptics.ScaleOption(intensity, duration);
            var rotationFront = new bHaptics.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyArm = "Recoil" + postfix;
            string keyVest = "RecoilVest" + postfix;
            bHaptics.SubmitRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHaptics.SubmitRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }

        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StartNeckTingle()
        {
            NeckTingle_mrse.Set();
        }

        public void StopNeckTingle()
        {
            NeckTingle_mrse.Reset();
        }

        public void StartTelekinesis(bool isRight)
        {
            if (isRight) { TelekinesisR_mrse.Set(); }
            else { TelekinesisL_mrse.Set(); }
        }

        public void StopTelekinesis(bool isRight)
        {
            if (isRight) { TelekinesisR_mrse.Reset(); StopHapticFeedback("Telekinesis_R"); }
            else { TelekinesisL_mrse.Reset(); StopHapticFeedback("Telekinesis_L"); }
        }

        public bool IsPlaying(String effect)
        {
            return bHaptics.IsPlaying(effect);
        }

        public void StopHapticFeedback(String effect)
        {
            bHaptics.TurnOff(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                bHaptics.TurnOff(key);
            }
        }

        public void StopThreads()
        {
            StopHeartBeat();
            StopNeckTingle();
            StopTelekinesis(true);
            StopTelekinesis(false);
        }


    }
}
