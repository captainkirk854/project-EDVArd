﻿namespace Bindings
{
    using System;
    using System.Data;
    using Helpers;

    /// <summary>
    /// Analyse Actions between VoiceAttack and Elite Dangerous ..
    /// </summary>
    public static class GameBindingsAnalyser
    {
        private const string NA = "n/a";
        private const string IntNA = "-2";

        /// <summary>
        /// Consolidate Command Key Codes in VoiceAttack based on Elite Dangerous Binds as Master ..
        /// </summary>
        /// <param name="voiceAttack"></param>
        /// <param name="eliteDangerous"></param>
        /// <returns></returns>
        public static DataTable ForUpdateInVoiceAttack(DataTable voiceAttack, DataTable eliteDangerous)
        {
            // Initialise lookup dictionary for inter-game action references ..
            CommandExchange actions = new CommandExchange();
            actions.Initialise();

            // Datatable to hold tabulated contents ..
            DataTable consolidatedaction = TableType.ConsolidatedActions();

            // Search through all defined Voice Attack bindings ..
            var voiceattackBindings = from va in voiceAttack.AsEnumerable()
                                    select
                                       new
                                         {
                                            KeyEnumeration = va.Field<string>(Enums.Column.KeyEnumeration.ToString()),
                                            EliteDangerousAction = actions.GetED(va.Field<string>(Enums.Column.KeyAction.ToString())),
                                            Action = va.Field<string>(Enums.Column.KeyAction.ToString()),
                                            KeyValue = va.Field<string>(Enums.Column.KeyEnumerationValue.ToString()),
                                            KeyCode = va.Field<int>(Enums.Column.KeyEnumerationCode.ToString()),
                                            KeyID = va.Field<string>(Enums.Column.KeyId.ToString()),
                                            FilePath = va.Field<string>(Enums.Column.FilePath.ToString()),
                                            Internal = va.Field<string>(Enums.Column.Internal.ToString())
                                         };

            // .. and compare with what has been defined in the Elite Dangerous bindings ..
            foreach (var voiceattackBinding in voiceattackBindings)
            {
                bool commandDefinedInEliteDangerousBindsFile = false;
                string remapRequired = "unknown";
                string rationale = "unknown";

                var elitedangerousBindings = from ed in eliteDangerous.AsEnumerable()
                                            where ed.Field<string>(Enums.Column.KeyAction.ToString()) == voiceattackBinding.EliteDangerousAction
                                           select
                                              new
                                                {
                                                    Action = ed.Field<string>(Enums.Column.KeyAction.ToString()),
                                                    KeyPriority = ed.Field<string>(Enums.Column.DevicePriority.ToString()),
                                                    KeyGameValue = ed.Field<string>(Enums.Column.KeyGameValue.ToString()),
                                                    KeyEnumerationValue = ed.Field<string>(Enums.Column.KeyEnumerationValue.ToString()),
                                                    KeyEnumerationCode = ed.Field<int>(Enums.Column.KeyEnumerationCode.ToString()),
                                                    KeyID = ed.Field<string>(Enums.Column.KeyId.ToString()),
                                                    ModifierKeyGameValue = ed.Field<string>(Enums.Column.ModifierKeyGameValue.ToString()),
                                                    ModifierKeyEnumerationValue = ed.Field<string>(Enums.Column.ModifierKeyEnumerationValue.ToString()),
                                                    ModifierKeyEnumerationCode = ed.Field<int>(Enums.Column.ModifierKeyEnumerationCode.ToString()),
                                                    ModifierKeyID = ed.Field<string>(Enums.Column.ModifierKeyId.ToString()),
                                                    FilePath = ed.Field<string>(Enums.Column.FilePath.ToString()),
                                                    Internal = ed.Field<string>(Enums.Column.Internal.ToString())
                                                };

                // Compare matching action bindings with their assigned key value/code ..
                foreach (var elitedangerousBinding in elitedangerousBindings)
                {
                    commandDefinedInEliteDangerousBindsFile = true;
                    if (elitedangerousBinding.KeyEnumerationCode == voiceattackBinding.KeyCode)
                    {
                        remapRequired = Enums.KeyUpdateRequired.NO.ToString();
                        rationale = "Key codes are aligned";
                    }
                    else
                    {
                        if (elitedangerousBinding.KeyEnumerationCode > 0)
                        {
                            remapRequired = Enums.KeyUpdateRequired.YES.ToString();
                            rationale = string.Format("Misaligned key codes: Voice Attack Profile requires change in key code from [{0}] to [{1}]", voiceattackBinding.KeyCode, elitedangerousBinding.KeyEnumerationCode);
                        }
                        else
                        {
                            remapRequired = Enums.KeyUpdateRequired.NO.ToString();
                            rationale = string.Format("Unresolvable key code for: [{0}]", elitedangerousBinding.KeyGameValue);
                        }
                    }

                    // Append to DataTable ..
                    consolidatedaction.LoadDataRow(new object[] 
                                                {
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.KeyEnumeration, //KeyEnumeration
                                                 ////--------------------------------------------------------------------------
                                                 remapRequired, //ReMapRequired
                                                 rationale, //Rationale
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.Action, //VoiceAttackAction
                                                 voiceattackBinding.EliteDangerousAction, //EliteDangerousAction
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.KeyValue, //VoiceAttackKeyValue
                                                 voiceattackBinding.KeyCode, //VoiceAttackKeyCode
                                                 voiceattackBinding.KeyID, //VoiceAttackKeyId
                                                 ////--------------------------------------------------------------------------                                                 
                                                 elitedangerousBinding.KeyPriority, //EliteDangerousDevicePriority
                                                 elitedangerousBinding.KeyGameValue, //EliteDangerousKeyValue
                                                 elitedangerousBinding.KeyEnumerationCode, //EliteDangerousKeyCode
                                                 elitedangerousBinding.KeyID, //EliteDangerousKeyId
                                                 elitedangerousBinding.ModifierKeyGameValue, //EliteDangerousModifierKeyValue
                                                 elitedangerousBinding.ModifierKeyEnumerationCode, //EliteDangerousModifierKeyCode
                                                 elitedangerousBinding.ModifierKeyID, //EliteDangerousModifierKeyId
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.Internal, //VoiceAttackInternal
                                                 voiceattackBinding.FilePath, //VoiceAttackProfile
                                                 elitedangerousBinding.Internal, //EliteDangerousInternal
                                                 elitedangerousBinding.FilePath //EliteDangerousFilePath
                                                 ////--------------------------------------------------------------------------
                                                }, 
                                                false);
                }

                // If not defined in Elite Dangerous binding file ..
                if (!commandDefinedInEliteDangerousBindsFile)
                {
                    // Append to DataTable
                    remapRequired = Enums.KeyUpdateRequired.NO.ToString();
                    rationale = string.Format("[{0}] has not been bound to a key", voiceattackBinding.EliteDangerousAction);
                    consolidatedaction.LoadDataRow(new object[] 
                                                {
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.KeyEnumeration, //KeyEnumeration
                                                 ////--------------------------------------------------------------------------
                                                 remapRequired, //ReMapRequired
                                                 rationale, //Rationale
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.Action, //VoiceAttackAction
                                                 voiceattackBinding.EliteDangerousAction, //EliteDangerousAction
                                                 ////--------------------------------------------------------------------------
                                                 voiceattackBinding.KeyValue, //VoiceAttackKeyValue
                                                 voiceattackBinding.KeyCode, //VoiceAttackKeyCode
                                                 voiceattackBinding.KeyID, //VoiceAttackKeyId
                                                 ////--------------------------------------------------------------------------
                                                 NA, //EliteDangerousDevicePriority                 
                                                 NA, //EliteDangerousKeyValue
                                                 IntNA, //EliteDangerousKeyCode
                                                 NA, //EliteDangerousKeyId
                                                 NA, //EliteDangerousModifierKeyValue
                                                 IntNA, //EliteDangerousModifierKeyCode
                                                 NA, //EliteDangerousModifierKeyId
                                                 ////--------------------------------------------------------------------------                                               
                                                 voiceattackBinding.Internal, //VoiceAttackInternal
                                                 voiceattackBinding.FilePath, //VoiceAttackProfile
                                                 NA, //EliteDangerousInternal
                                                 NA //EliteDangerousFilePath
                                                 ////--------------------------------------------------------------------------
                                                },
                                                false);
                }
            }

            return consolidatedaction;
        }
    }
}