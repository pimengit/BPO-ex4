using BPO_ex4.LogicSheets;
using System;

namespace BPO_ex4.StationLogic
{
    public static class SourceRules
    {
        public static string Resolve(
            string targetSheet,
            string ist,
            int? num)
        {
            // =========================
            // КОНСТАНТЫ
            // =========================
            if (ist == "3.0") return "CONST_0";
            if (ist == "3.1") return "CONST_1";

            // =========================
            // BELL_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("BELL_"))
            {
                if (num == null)
                    throw new Exception($"BELL: missing controller for ist={ist}");


                // OUT: 0.x
                if (ist.StartsWith("0."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"BELL_OUT[{num}:{ch}]";
                }


                throw new Exception($"BELL: unknown ist={ist}");
            }



            // =========================
            // ROUTESECT_*
            // =========================
            if (targetSheet.StartsWith("ROUTESECT_"))
            {
                if (num == null)
                    throw new Exception($"ROUTESECT: missing num for ist={ist}");

                if (ist == "2.0")
                    return $"SWITCH_MK[{num}]";

                if (ist == "2.1")
                    return $"SWITCH_PK[{num}]";

                if (ist == "2.2")
                    return $"ROUTESECT_OPz[{num}]";

                if (ist == "2.3")
                    return $"ROUTE_SU[{num}]";

                if (ist == "2.4")
                    return $"AVTOSTOP_AZ[{num}]";

                if (ist == "2.5")
                    return $"SPEED_OS0[{num}]";

                if (ist == "2.6")
                    return $"PARTROUTE_Uch[{num}]";

                if (ist == "2.7")
                    return $"ROUTESECT_PL1[{num}]";

                if (ist == "2.8")
                    return $"ROUTESECT_PL2[{num}]";

                if (ist == "2.9")
                    return $"ROUTESECT_PL3[{num}]";

                if (ist == "2.10")
                    return $"ROUTESECT_UMP[{num}]";

                if (ist == "2.11")
                    return $"SIGNAL_VU[{num}]";

                if (ist == "2.12")
                    return $"GEN_ROG[{num}]";

                if (ist == "2.13")
                    return $"SECURITY_RU[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "3.3")
                    return $"SECT_Lz[{num}]";

                if (ist == "3.4")
                    return $"SECT_LS[{num}]";

                if (ist == "3.5")
                    return $"SECT_zD125[{num}]";

                if (ist == "3.6")
                    return $"SECT_zD13[{num}]";

                if (ist == "3.7")
                    return $"SECT_zD45[{num}]";

                if (ist == "3.14")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.0")
                    return $"STAGE_CHN[{num}]";

                if (ist == "4.1")
                    return $"STAGE_NN[{num}]";

                if (ist == "4.9")
                    return $"ROUTESECT_40NUM[{num}]";

                if (ist == "4.10")
                    return $"ROUTESECT_40NUM1[{num}]";

                if (ist == "4.11")
                    return $"ROUTESECT_40NUM2[{num}]";

                if (ist == "4.12")
                    return $"ROUTESECT_40NUM3[{num}]";

                if (ist == "4.13")
                    return $"ROUTESECT_40NUM4[{num}]";

                if (ist == "4.14")
                    return $"ROUTESECT_40NUM5[{num}]";

                if (ist == "4.15")
                    return $"ROUTESECT_40NUM6[{num}]";

                if (ist == "5.3")
                   return $"IS_KS[{num}]";

                if (ist == "5.9")
                    return $"ROUTESECT_40CUM[{num}]";

                if (ist == "5.10")
                    return $"ROUTESECT_40CUM1[{num}]";

                if (ist == "5.11")
                    return $"ROUTESECT_40CUM2[{num}]";

                if (ist == "5.12")
                    return $"ROUTESECT_40CUM3[{num}]";

                if (ist == "5.13")
                    return $"ROUTESECT_40CUM4[{num}]";

                if (ist == "5.14")
                    return $"ROUTESECT_40CUM5[{num}]";

                if (ist == "5.15")
                    return $"ROUTESECT_40CUM6[{num}]";

                if (ist == "6.15")
                    return $"ROUTESECT_CPz6[{num}]";

                if (ist == "6.14")
                    return $"ROUTESECT_CPz5[{num}]";

                if (ist == "6.13")
                    return $"ROUTESECT_CPz4[{num}]";

                if (ist == "6.12")
                    return $"ROUTESECT_CPz3[{num}]";

                if (ist == "6.11")
                    return $"ROUTESECT_CPz2[{num}]";

                if (ist == "6.10")
                    return $"ROUTESECT_CPz1[{num}]";

                if (ist == "7.15")
                    return $"ROUTESECT_NPz6[{num}]";

                if (ist == "7.14")
                    return $"ROUTESECT_NPz5[{num}]";

                if (ist == "7.13")
                    return $"ROUTESECT_NPz4[{num}]";

                if (ist == "7.12")
                    return $"ROUTESECT_NPz3[{num}]";

                if (ist == "7.11")
                    return $"ROUTESECT_NPz2[{num}]";

                if (ist == "7.10")
                    return $"ROUTESECT_NPz1[{num}]";

                if (ist == "7.4")
                    return $"ROUTE_z_M[{num}]";

                if (ist == "7.2")
                    return $"ROUTE_z_Mdop[{num}]";

                if (ist == "7.1")
                    return $"SECT_MSP[{num}]";


                throw new Exception($"ROUTESECT: unknown ist={ist}");
            }

            // =========================
            // RELAY_*  (IN / OUT)
            // =========================
            if (targetSheet.StartsWith("RELAY_"))
            {
                if (num == null)
                    throw new Exception($"RELAY: missing controller for ist={ist}");

                // IN: 0.x , 1.x
                if (ist.StartsWith("0.") || ist.StartsWith("1."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"RELAY_IN[{num}:{ch}]";
                }

                // OUT: 2.x
                if (ist.StartsWith("2."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"RELAY_OUT[{num}:{ch}]";
                }

                // логические связи
                if (ist == "3.2")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "3.3")
                    return $"RELAY_NRK1[{num}]";

                if (ist == "3.4")
                    return $"RELAY_KTK[{num}]";

                throw new Exception($"RELAY: unknown ist={ist}");
            }

            // =========================
            // SECT_*  (IN / OUT)
            // =========================
            if (targetSheet.StartsWith("SECT_"))
            {
                if (num == null)
                    throw new Exception($"SECT: missing controller for ist={ist}");

                // IN: 0.x , 1.x
                if (ist.StartsWith("0."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_IN[{num}:{ch}]";
                }

                // Команды
                if (ist=="1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_CAVN_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_UPN_MK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_OUPN_MK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_SLS_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_VN_DK[{num}:{ch}]";
                }

                if (ist == "1.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_BRC_DK[{num}:{ch}]";
                }

                if (ist == "1.6")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_OBRC_DK[{num}:{ch}]";
                }

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI4[{num}:{ch}]";
                }

                // логические связи
                if (ist == "2.15")
                    return $"GEN_ON[{num}]";

                if (ist == "2.14")
                    return $"STAGE_OKL[{num}]";

                if (ist == "2.13")
                    return $"GEN_ORO[{num}]";

                if (ist == "2.12")
                    return $"GEN_ROG[{num}]";

                if (ist == "2.11")
                    return $"GEN_ROPS[{num}]";

                if (ist == "2.7")
                    return $"SECT_CAVN[{num}]";

                if (ist == "2.6")
                    return $"DC_CAV[{num}]";

                if (ist == "2.5")
                    return $"KOD_OD2[{num}]";

                if (ist == "2.4")
                    return $"ROUTE_MA[{num}]";

                if (ist == "2.3")
                    return $"ROUTE_Pz_M[{num}]";

                if (ist == "2.2")
                    return $"ROUTESECT_OPz[{num}]";

                if (ist == "2.1")
                    return $"SWITCH_PK[{num}]";

                if (ist == "2.0")
                    return $"SWITCH_MK[{num}]";

                if (ist == "3.15")
                    return $"DC_DC[{num}]";

                if (ist == "3.14")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "3.9")
                    return $"SECT_OUPN[{num}]";

                if (ist == "3.8")
                    return $"SECT_UPN[{num}]";

                if (ist == "3.7")
                    return $"ROUTESECT_OPz[{num}]";

                if (ist == "3.6")
                    return $"SECT_PNRO[{num}]";

                if (ist == "3.5")
                    return $"SECT_SLS[{num}]";

                if (ist == "3.4")
                    return $"SECT_LS[{num}]";

                if (ist == "3.3")
                    return $"SECT_Lz[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.15")
                    return $"SECT_LS125[{num}]";

                if (ist == "4.14")
                    return $"SECT_LS13[{num}]";

                if (ist == "4.13")
                    return $"SECT_LS45[{num}]";

                if (ist == "5.15")
                    return $"SECT_zD125[{num}]";

                if (ist == "5.14")
                    return $"SECT_zD13[{num}]";

                if (ist == "5.13")
                    return $"SECT_zD45[{num}]";

                if (ist == "5.6")
                    return $"SECT_VPN[{num}]";

                if (ist == "5.5")
                    return $"KOD_KNOD[{num}]";

                if (ist == "5.4")
                    return $"KOD_PNT[{num}]";

                if (ist == "5.2")
                    return $"SECT_OBRC[{num}]";

                if (ist == "5.1")
                    return $"SECT_BRC[{num}]";

                if (ist == "5.0")
                    return $"SECT_MAP[{num}]";

                if (ist == "7.9")
                    return $"SPEED_80U[{num}]";

                if (ist == "7.8")
                    return $"SPEED_70U[{num}]";

                if (ist == "7.7")
                    return $"SPEED_60U[{num}]";

                if (ist == "7.6")
                    return $"SPEED_40U[{num}]";

                if (ist == "7.5")
                    return $"ROUTESECT_40NUM[{num}]";

                if (ist == "7.4")
                    return $"ROUTESECT_40CUM[{num}]";

                if (ist == "7.3")
                    return $"SPEED_HV[{num}]";

                if (ist == "7.2")
                    return $"ROUTE_z_M[{num}]";

                if (ist == "7.1")
                    return $"ROUTE_KN[{num}]";

                if (ist == "7.0")
                    return $"SECT_PN[{num}]";




                throw new Exception($"SECT: unknown ist={ist}");
            }


            // =========================
            // KGU_*  (IN / OUT)
            // =========================
            if (targetSheet.StartsWith("KGU_"))
            {
                if (num == null)
                    throw new Exception($"KGU: missing controller for ist={ist}");



                // Команды
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KGU_VKGU_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KGU_OVKGU_DK[{num}:{ch}]";

                }

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.7")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.2")
                    return $"KGU_OVKGU[{num}]";

                if (ist == "5.7")
                    return $"KGU_VKGU[{num}]";

                if (ist == "5.6")
                    return $"KGU_AKGU[{num}]";

                if (ist == "5.5")
                    return $"KGU_PKGU[{num}]";

                if (ist == "5.4")
                    return $"KGU_OKGU[{num}]";

                if (ist == "6.7")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "6.1")
                    return $"KGU_KKGU[{num}]";

                throw new Exception($"KGU: unknown ist={ist}");
            }



            // =========================
            // AVTOSTOP_*
            // =========================
            if (targetSheet.StartsWith("AVTOSTOP_"))
            {
                if (num == null)
                    throw new Exception($"AVTOSTOP: missing num for ist={ist}");

                if (ist == "3.15")
                    return $"AVTOSTOP_VAR1[{num}]";

                if (ist == "3.14")
                    return $"AVTOSTOP_VARCH[{num}]";

                if (ist == "3.13")
                    return $"AVTOSTOP_VAR[{num}]";

                if (ist == "3.11")
                    return $"SIGNAL_PS2[{num}]";

                if (ist == "3.10")
                    return $"AVTOSTOP_PSVA[{num}]";

                if (ist == "3.9")
                    return $"AVTOSTOP_VA[{num}]";

                if (ist == "3.8")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "3.7")
                    return $"AVTOSTOP_OA[{num}]";

                if (ist == "3.6")
                    return $"AVTOSTOP_zA[{num}]";

                if (ist == "3.5")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "3.4")
                    return $"SIGNAL_PS[{num}]";

                if (ist == "3.3")
                    return $"ROUTE_VAM[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.15")
                    return $"AVTOSTOP_VAR0[{num}]";

                if (ist == "4.14")
                    return $"AVTOSTOP_PAS[{num}]";

                if (ist == "4.13")
                    return $"RELAY_KTK[{num}]";

                throw new Exception($"AVTOSTOP: unknown ist={ist}");
            }




            // =========================
            // METALL_*
            // =========================
            if (targetSheet.StartsWith("METALL_"))
            {
                if (num == null)
                    throw new Exception($"METALL: missing num for ist={ist}");

                if (ist == "3.15")
                    return $"METALL_MKKV[{num}]";

                if (ist == "3.14")
                    return $"METALL_MKR[{num}]";

                if (ist == "4.13")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"METALL_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"METALL_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"METALL_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"METALL_EI4[{num}:{ch}]";
                }

                throw new Exception($"METALL: unknown ist={ist}");
            }



            // =========================
            // STAGE_*
            // =========================
            if (targetSheet.StartsWith("STAGE_"))
            {
                if (num == null)
                    throw new Exception($"STAGE: missing num for ist={ist}");

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_EI4[{num}:{ch}]";
                }


                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_VSAB_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_VSARS_DK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_DS_DK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_CDS_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_CAK_DK[{num}:{ch}]";
                }

                if (ist == "1.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_ANK_DK[{num}:{ch}]";
                }

                if (ist == "1.6")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_NK_DK[{num}:{ch}]";
                }

                if (ist == "1.7")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_ACHK_DK[{num}:{ch}]";
                }

                if (ist == "1.8")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_CHK_DK[{num}:{ch}]";
                }

                if (ist == "1.9")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"STAGE_ONCHK_DK[{num}:{ch}]";
                }

                if (ist == "2.15")
                    return $"AVTODO_AR[{num}]";

                if (ist == "2.14")
                    return $"SIGNAL_KO[{num}]";

                if (ist == "2.13")
                    return $"STAGE_VSAB[{num}]";

                if (ist == "2.12")
                    return $"STAGE_VS[{num}]";

                if (ist == "2.11")
                    return $"SPEED_FH[{num}]";

                if (ist == "2.10")
                    return $"SPEED_80U[{num}]";

                if (ist == "2.9")
                    return $"SPEED_70U[{num}]";

                if (ist == "2.8")
                    return $"SPEED_60U[{num}]";

                if (ist == "2.7")
                    return $"SPEED_40U[{num}]";

                if (ist == "2.6")
                    return $"STAGE_PRZD[{num}]";

                if (ist == "2.5")
                    return $"STAGE_PRB[{num}]";

                if (ist == "2.4")
                    return $"STAGE_ZS[{num}]";

                if (ist == "2.3")
                    return $"STAGE_ZSD[{num}]";

                if (ist == "2.2")
                    return $"SIGNAL_BSV[{num}]";

                if (ist == "2.1")
                    return $"SECT_BRC[{num}]";

                if (ist == "2.0")
                    return $"SECT_P[{num}]";

                if (ist == "3.15")
                    return $"STAGE_ZSR[{num}]";

                if (ist == "3.14")
                    return $"STAGE_CDS[{num}]";

                if (ist == "3.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "3.12")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "3.11")
                    return $"ROUTE_SU[{num}]";

                if (ist == "3.10")
                    return $"DC_CPS[{num}]";

                if (ist == "3.9")
                    return $"STAGE_SGLS[{num}]";

                if (ist == "3.8")
                    return $"SIGNAL_PS[{num}]";

                if (ist == "3.7")
                    return $"SIGNAL_PSO[{num}]";

                if (ist == "3.6")
                    return $"ROUTE_PSMSTR[{num}]";

                if (ist == "3.5")
                    return $"SIGGROUP_KP1[{num}]";

                if (ist == "3.4")
                    return $"SIGGROUP_KP2[{num}]";

                if (ist == "3.3")
                    return $"STAGE_DS[{num}]";

                if (ist == "3.2")
                    return $"SECT_MSP[{num}]";

                if (ist == "4.15")
                    return $"SIGGROUP_KP[{num}]";

                if (ist == "4.14")
                    return $"STAGE_SVP[{num}]";

                if (ist == "4.13")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.12")
                    return $"DC_CAN[{num}]";

                if (ist == "4.11")
                    return $"DC_RMU[{num}]";

                if (ist == "4.10")
                    return $"STAGE_CAK[{num}]";

                if (ist == "4.9")
                    return $"SECT_zD125[{num}]";

                if (ist == "4.8")
                    return $"PARTROUTE_Zc[{num}]";

                if (ist == "4.7")
                    return $"STAGE_CRM[{num}]";

                if (ist == "4.6")
                    return $"STAGE_SVPSN[{num}]";

                if (ist == "4.5")
                    return $"CABEL_KKL[{num}]";

                if (ist == "4.4")
                    return $"STAGE_NN[{num}]";

                if (ist == "4.3")
                    return $"STAGE_NND[{num}]";

                if (ist == "4.2")
                    return $"STAGE_ANK[{num}]";

                if (ist == "4.1")
                    return $"STAGE_NK[{num}]";

                if (ist == "4.0")
                    return $"STAGE_PUT[{num}]";

                if (ist == "5.15")
                    return $"STAGE_NKD[{num}]";

                if (ist == "5.14")
                    return $"STAGE_ANKD[{num}]";

                if (ist == "5.13")
                    return $"STAGE_CHKD[{num}]";

                if (ist == "5.12")
                    return $"STAGE_ACHKD[{num}]";

                if (ist == "5.11")
                    return $"STAGE_NM[{num}]";

                if (ist == "5.10")
                    return $"STAGE_CHM[{num}]";

                if (ist == "5.9")
                    return $"RELAY_KTK[{num}]";

                if (ist == "5.6")
                    return $"STAGE_SVPM[{num}]";

                if (ist == "5.5")
                    return $"SIGNAL_PS2[{num}]";

                if (ist == "5.4")
                    return $"STAGE_CHN[{num}]";

                if (ist == "5.3")
                    return $"STAGE_CND[{num}]";

                if (ist == "5.2")
                    return $"STAGE_ACHK[{num}]";

                if (ist == "5.1")
                    return $"STAGE_CHK[{num}]";

                if (ist == "5.0")
                    return $"STAGE_ONCHK[{num}]";


                throw new Exception($"STAGE: unknown ist={ist}");
            }


            // =========================
            // KURBEL_*
            // =========================
            if (targetSheet.StartsWith("KURBEL_"))
            {
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KURBEL_KKUD_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KURBEL_KKU_DK[{num}:{ch}]";
                }

                if (ist == "3.15")
                    return $"DC_CAV[{num}]";

                if (ist == "3.14")
                    return $"DC_DC[{num}]";

                if (ist == "3.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "3.2")
                    return $"KOD_PNT[{num}]";

                if (ist == "4.15")
                    return $"KURBEL_KKUK[{num}]";

                if (ist == "4.14")
                    return $"KURBEL_KKU[{num}]";

                if (ist == "4.13")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.12")
                    return $"KURBEL_KKUD[{num}]";

                if (ist == "5.1")
                    return $"KOD_KNOD[{num}]";

                throw new Exception($"KURBEL: unknown ist={ist}");
            }


            // =========================
            // IS_*
            // =========================
            if (targetSheet.StartsWith("IS_"))
            {
                if (num == null)
                    throw new Exception($"IS: missing num for ist={ist}");

                if (ist == "2.2")
                    return $"STAGE_OKL[{num}]";

                if (ist == "2.1")
                    return $"AGP_NAGP[{num}]";

                if (ist == "2.0")
                    return $"SECT_P[{num}]";

                if (ist == "4.13")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.6")
                    return $"IS_KS[{num}]";

                if (ist == "4.2")
                    return $"IS_Tks[{num}]";

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"IS_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"IS_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"IS_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"IS_EI4[{num}:{ch}]";
                }

                throw new Exception($"IS: unknown ist={ist}");
            }


            // =========================
            // SWITCH_*
            // =========================
            if (targetSheet.StartsWith("SWITCH_"))
            {
                if (num == null)
                    throw new Exception($"SWITCH: missing num for ist={ist}");

                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_VMU_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_MU_DK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_VPU_DK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_PU_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_OKLP_DK[{num}:{ch}]";
                }

                if (ist == "1.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_KLP_DK[{num}:{ch}]";
                }

                if (ist == "1.6")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_MS_DK[{num}:{ch}]";
                }

                if (ist == "1.7")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_MS_MK[{num}:{ch}]";
                }

                if (ist == "1.8")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_ORK_DK[{num}:{ch}]";
                }

                if (ist == "1.9")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_RK_DK[{num}:{ch}]";
                }

                if (ist == "1.10")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_SSK_DK[{num}:{ch}]";
                }

                if (ist == "1.11")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_VRK_MK[{num}:{ch}]";
                }

                if (ist == "1.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_OVRK_MK[{num}:{ch}]";
                }

                if (ist == "1.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_OKV_DK[{num}:{ch}]";
                }

                if (ist == "1.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_KV_DK[{num}:{ch}]";
                }

                if (ist == "3.15")
                    return $"SWITCH_PPA[{num}]";

                if (ist == "3.14")
                    return $"SWITCH_PMA[{num}]";

                if (ist == "3.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "3.12")
                    return $"SWITCH_PA[{num}]";

                if (ist == "3.11")
                    return $"SWITCH_SK[{num}]";

                if (ist == "3.10")
                    return $"SWITCH_SSK[{num}]";

                if (ist == "3.9")
                    return $"SWITCH_PKT[{num}]";

                if (ist == "3.8")
                    return $"MAKET_t2M[{num}]";

                if (ist == "3.7")
                    return $"SWITCH_Tp[{num}]";

                if (ist == "3.6")
                    return $"SWITCH_MS[{num}]";

                if (ist == "3.5")
                    return $"SWITCH_MPS[{num}]";

                if (ist == "3.4")
                    return $"SWITCH_MMS[{num}]";

                if (ist == "3.3")
                    return $"SWITCH_KV[{num}]";

                if (ist == "3.2")
                    return $"SWITCH_KLP[{num}]";

                if (ist == "4.15")
                    return $"SWITCH_PSVP[{num}]";

                if (ist == "4.14")
                    return $"MAKET_OMV[{num}]";

                if (ist == "4.13")
                    return $"SWITCH_PUM[{num}]";

                if (ist == "4.12")
                    return $"SWITCH_MUM[{num}]";

                if (ist == "4.11")
                    return $"SWITCH_PK[{num}]";

                if (ist == "4.10")
                    return $"SWITCH_MK[{num}]";

                if (ist == "4.9")
                    return $"SWITCH_PU[{num}]";

                if (ist == "4.8")
                    return $"SWITCH_MU[{num}]";

                if (ist == "4.7")
                    return $"SWITCH_VPU[{num}]";

                if (ist == "4.6")
                    return $"SWITCH_VMU[{num}]";

                if (ist == "4.5")
                    return $"SWITCH_zStr[{num}]";

                if (ist == "4.4")
                    return $"MAKET_MP[{num}]";

                if (ist == "4.3")
                    return $"MAKET_MM[{num}]";

                if (ist == "4.2")
                    return $"SWITCH_VPS[{num}]";

                if (ist == "4.1")
                    return $"SWITCH_ORK[{num}]";

                if (ist == "4.0")
                    return $"SWITCH_RK[{num}]";

                if (ist == "5.15")
                    return $"SWITCH_PSVM[{num}]";

                if (ist == "5.14")
                    return $"SWITCH_VKS[{num}]";

                if (ist == "5.13")
                    return $"SWITCH_MSM[{num}]";

                if (ist == "5.12")
                    return $"SWITCH_MSD[{num}]";

                if (ist == "5.11")
                    return $"SWITCH_AOIS[{num}]";

                if (ist == "5.10")
                    return $"SWITCH_PZN[{num}]";

                if (ist == "5.9")
                    return $"SWITCH_PD[{num}]";

                if (ist == "5.8")
                    return $"SWITCH_MD[{num}]";

                if (ist == "5.7")
                    return $"SWITCH_OKV[{num}]";

                if (ist == "5.6")
                    return $"SWITCH_OKLP[{num}]";

                if (ist == "5.5")
                    return $"SWITCH_PSTR[{num}]";

                if (ist == "5.4")
                    return $"MAKET_MV[{num}]";

                if (ist == "5.3")
                    return $"ROUTE_KN[{num}]";

                if (ist == "5.2")
                    return $"SECT_P[{num}]";

                if (ist == "5.1")
                    return $"SECURITY_OHR[{num}]";

                if (ist == "5.0")
                    return $"AVTODO_AR[{num}]";

                if (ist == "6.15")
                    return $"SWITCH_VPNOST[{num}]";

                if (ist == "6.14")
                    return $"SWITCH_VPNRST[{num}]";

                if (ist == "7.15")
                    return $"SWITCH_VRK[{num}]";

                if (ist == "7.14")
                    return $"PARTROUTE_Zc[{num}]";

                if (ist == "7.13")
                    return $"ROUTESECT_PL3[{num}]";

                if (ist == "7.12")
                    return $"ROUTESECT_PL2[{num}]";

                if (ist == "7.11")
                    return $"ROUTESECT_PL1[{num}]";

                if (ist == "7.10")
                    return $"SWITCH_TOS[{num}]";

                if (ist == "7.9")
                    return $"SWITCH_PARK[{num}]";

                if (ist == "7.8")
                    return $"SWITCH_SRK[{num}]";

                if (ist == "7.7")
                    return $"SWITCH_OVRK[{num}]";

                if (ist == "7.6")
                    return $"MAKET_MI[{num}]";

                if (ist == "7.5")
                    return $"SECT_MSP[{num}]";

                if (ist == "7.4")
                    return $"ROUTE_PSMSTR[{num}]";

                if (ist == "7.3")
                    return $"SWITCH_UM[{num}]";

                if (ist == "7.2")
                    return $"SWITCH_T1os[{num}]";

                if (ist.StartsWith("0."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SWITCH_IN[{num}:{ch}]";
                }




                throw new Exception($"SWITCH: unknown ist={ist}");
            }

            // =========================
            // AVTODO_*  
            // =========================
            if (targetSheet.StartsWith("AVTODO_"))
            {
                if (num == null)
                    throw new Exception($"AVTODO: missing controller for ist={ist}");


                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AVTODO_AR_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AVTODO_OAR_DK[{num}:{ch}]";
                }

                if (ist == "2.10")
                    return $"AVTODO_ASC[{num}]";

                if (ist == "2.9")
                    return $"AVTODO_TSC[{num}]";

                if (ist == "2.8")
                    return $"AVTODO_OSC[{num}]";

                if (ist == "2.7")
                    return $"AVTODO_SMAR[{num}]";

                if (ist == "2.6")
                    return $"AVTODO_MAR[{num}]";

                if (ist == "2.5")
                    return $"AVTODO_SC[{num}]";

                if (ist == "2.4")
                    return $"SIGNAL_APS[{num}]";

                if (ist == "2.3")
                    return $"AVTODO_ARUS[{num}]";

                if (ist == "2.2")
                    return $"ROUTE_SZ[{num}]";

                if (ist == "2.1")
                    return $"KN_DKN[{num}]";

                if (ist == "2.0")
                    return $"KN_DMKNO[{num}]";

                if (ist == "3.15")
                    return $"ROUTE_KNO[{num}]";

                if (ist == "3.14")
                    return $"ROUTE_KN[{num}]";

                if (ist == "3.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "3.8")
                    return $"ROUTE_MGP[{num}]";

                if (ist == "3.7")
                    return $"AVTODO_34AZ[{num}]";

                if (ist == "3.6")
                    return $"AVTODO_SAR[{num}]";

                if (ist == "3.5")
                    return $"AVTODO_GSAZ[{num}]";

                if (ist == "3.4")
                    return $"AVTODO_SCAR[{num}]";

                if (ist == "3.3")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.15")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "4.14")
                    return $"DC_DOSK[{num}]";

                if (ist == "4.13")
                    return $"DC_DOK[{num}]";

                if (ist == "4.6")
                    return $"AVTODO_TAOP[{num}]";

                if (ist == "4.5")
                    return $"AVTODO_AOP[{num}]";

                if (ist == "4.3")
                    return $"AVTODO_DARsb[{num}]";

                if (ist == "4.2")
                    return $"AVTODO_DAR[{num}]";

                if (ist == "4.1")
                    return $"AVTODO_ADY[{num}]";

                if (ist == "4.0")
                    return $"ROUTE_PP[{num}]";

                if (ist == "5.5")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "5.4")
                    return $"ROUTE_GS[{num}]";

                if (ist == "6.7")
                    return $"AVTODO_OAR[{num}]";

                if (ist == "7.10")
                    return $"AVTODO_AR[{num}]";

                if (ist == "7.9")
                    return $"AVTODO_ACH[{num}]";

                if (ist == "7.1")
                    return $"ROUTE_vS[{num}]";

                throw new Exception($"AVTODO: unknown ist={ist}");
            }


            // =========================
            // ROUTE_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("ROUTE_"))
            {
                if (num == null)
                    throw new Exception($"ROUTE: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ROUTE_KNO_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ROUTE_KN_DK[{num}:{ch}]";
                }

                if (ist == "2.2")
                    return $"DC_DC[{num}]";

                if (ist == "2.1")
                    return $"KN_DKN[{num}]";

                if (ist == "2.0")
                    return $"KN_DMKNO[{num}]";

                if (ist == "3.15")
                    return $"ROUTE_KNO[{num}]";

                if (ist == "3.14")
                    return $"ROUTE_KN[{num}]";

                if (ist == "3.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "3.12")
                    return $"PARTROUTE_STRch[{num}]";

                if (ist == "3.11")
                    return $"SIGGROUP_IA[{num}]";

                if (ist == "3.10")
                    return $"ROUTE_Pz_M[{num}]";

                if (ist == "3.9")
                    return $"ROUTE_MSTR[{num}]";

                if (ist == "3.8")
                    return $"ROUTE_z_M[{num}]";

                if (ist == "3.7")
                    return $"SIGNAL_BSV[{num}]";

                if (ist == "3.6")
                    return $"ROUTE_z_Mdop[{num}]";

                if (ist == "3.5")
                    return $"SECT_MSP[{num}]";

                if (ist == "3.4")
                    return $"AVTOSTOP_AZ[{num}]";

                if (ist == "3.3")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.15")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "4.14")
                    return $"SWITCH_PZN[{num}]";

                if (ist == "4.13")
                    return $"SWITCH_SZM[{num}]";

                if (ist == "4.12")
                    return $"SWITCH_SZP[{num}]";

                if (ist == "4.11")
                    return $"ROUTE_MPz[{num}]";

                if (ist == "4.10")
                    return $"SIGGROUP_RI[{num}]";

                if (ist == "4.9")
                    return $"ROUTE_SZ[{num}]";

                if (ist == "4.8")
                    return $"SECURITY_OHR[{num}]";

                if (ist == "4.7")
                    return $"SECT_BRC[{num}]";

                if (ist == "4.6")
                    return $"SWITCH_PKT[{num}]";

                if (ist == "4.5")
                    return $"AVTODO_TSC[{num}]";

                if (ist == "4.4")
                    return $"AVTODO_AROP[{num}]";

                if (ist == "4.3")
                    return $"AVTODO_ARTOP[{num}]";

                if (ist == "4.2")
                    return $"SIGNAL_APS[{num}]";

                if (ist == "4.1")
                    return $"SWITCH_KLP[{num}]";

                if (ist == "4.0")
                    return $"ROUTE_OZM[{num}]";

                if (ist == "5.15")
                    return $"ROUTE_SU[{num}]";

                if (ist == "5.14")
                    return $"ROUTE_vS[{num}]";

                if (ist == "5.13")
                    return $"ROUTE_S[{num}]";

                if (ist == "5.12")
                    return $"ROUTE_UBM[{num}]";

                if (ist == "5.11")
                    return $"PARTROUTE_M2[{num}]";

                if (ist == "5.10")
                    return $"PARTROUTE_M1[{num}]";

                if (ist == "5.9")
                    return $"ROUTE_MGP[{num}]";

                if (ist == "5.8")
                    return $"ROUTE_AN[{num}]";

                if (ist == "5.7")
                    return $"SIGNAL_OMO[{num}]";

                if (ist == "5.6")
                    return $"ROUTE_PP[{num}]";

                if (ist == "5.5")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "5.4")
                    return $"ROUTE_GS[{num}]";

                if (ist == "5.3")
                    return $"SIGGROUP_KP1[{num}]";

                if (ist == "5.2")
                    return $"SWITCH_PK[{num}]";

                if (ist == "5.1")
                    return $"SWITCH_MK[{num}]";

                if (ist == "5.0")
                    return $"SWITCH_MS[{num}]";

                if (ist == "6.15")
                    return $"SIGNAL_PSO[{num}]";

                if (ist == "6.14")
                    return $"SIGNAL_RO_AB[{num}]";

                if (ist == "6.13")
                    return $"STAGE_NN[{num}]";

                if (ist == "6.12")
                    return $"STAGE_CHN[{num}]";

                if (ist == "6.11")
                    return $"SIGNAL_PS2[{num}]";

                if (ist == "6.6")
                    return $"METALL_MKBV[{num}]";

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI4[{num}:{ch}]";
                }

                if (ist == "7.11")
                    return $"PARTROUTE_Zc[{num}]";

                if (ist == "7.10")
                    return $"AVTODO_AR[{num}]";

                if (ist == "7.9")
                    return $"AVTODO_ACH[{num}]";

                if (ist == "7.8")
                    return $"SIGNAL_PS[{num}]";

                if (ist == "7.7")
                    return $"SIGNAL_KO[{num}]";

                if (ist == "7.6")
                    return $"SIGNAL_RO_ARS[{num}]";

                if (ist == "7.5")
                    return $"SIGNAL_1zHO[{num}]";

                if (ist == "7.4")
                    return $"SIGNAL_2zHO[{num}]";

                if (ist == "7.3")
                    return $"STAGE_SVP[{num}]";

                if (ist == "7.2")
                    return $"METALL_KVR[{num}]";

                if (ist == "7.1")
                    return $"STAGE_VS[{num}]";

                if (ist == "7.0")
                    return $"KGU_KKGU[{num}]";


                throw new Exception($"ROUTE: unknown ist={ist}");
            }




            // =========================
            // PARTROUTE_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("PARTROUTE_"))
            {
                if (num == null)
                    throw new Exception($"PARTROUTE: missing controller for ist={ist}");

                // OUT: 0.x
                if (ist == "3.15")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "3.14")
                    return $"ROUTE_MSTR[{num}]";

                if (ist == "3.13")
                    return $"ROUTE_SU[{num}]";

                if (ist == "3.11")
                    return $"ROUTE_GS[{num}]";

                if (ist == "3.10")
                    return $"SIGGROUP_IA[{num}]";

                if (ist == "3.9")
                    return $"SIGGROUP_RI[{num}]";

                if (ist == "3.7")
                    return $"PARTROUTE_M1[{num}]";

                if (ist == "3.6")
                    return $"PARTROUTE_M2[{num}]";

                if (ist == "3.5")
                    return $"SIGGROUP_KP1[{num}]";

                if (ist == "3.4")
                    return $"SIGGROUP_KP2[{num}]";

                if (ist == "3.3")
                    return $"SIGGROUP_KPS[{num}]";

                if (ist == "3.2")
                    return $"ALARMSIG_VzS[{num}]";

                if (ist == "4.8")
                    return $"AVTODO_AR[{num}]";

                if (ist == "4.3")
                    return $"ROUTE_MPz[{num}]";

                if (ist == "4.1")
                    return $"SECT_MSP[{num}]";

                if (ist == "4.0")
                    return $"SECT_P[{num}]";

                if (ist == "5.4")
                    return $"GEN_ROG[{num}]";

                if (ist == "5.3")
                    return $"PARTROUTE_Zc[{num}]";

                if (ist == "5.2")
                    return $"SWITCH_PK[{num}]";

                if (ist == "5.1")
                    return $"SWITCH_MK[{num}]";

                if (ist == "5.0")
                    return $"PARTROUTE_STRch[{num}]";

                if (ist == "6.11")
                    return $"SECURITY_OHR[{num}]";


                throw new Exception($"PARTROUTE: unknown ist={ist}");
            }





            // =========================
            // SIGGROUP_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("SIGGROUP_"))
            {
                if (num == null)
                    throw new Exception($"SIGGROUP: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGGROUP_IR_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGGROUP_CIR_DK[{num}:{ch}]";
                }

                if (ist == "3.13")
                    return $"ROUTE_KN[{num}]";

                if (ist == "3.12")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "3.11")
                    return $"ROUTE_GS[{num}]";

                if (ist == "3.10")
                    return $"SIGGROUP_IA[{num}]";

                if (ist == "3.9")
                    return $"SIGGROUP_RI[{num}]";

                if (ist == "3.8")
                    return $"ROUTE_PP[{num}]";

                if (ist == "3.7")
                    return $"PARTROUTE_M1[{num}]";

                if (ist == "3.6")
                    return $"PARTROUTE_M2[{num}]";

                if (ist == "3.5")
                    return $"SIGGROUP_KP2[{num}]";

                if (ist == "3.4")
                    return $"SIGGROUP_KP[{num}]";

                if (ist == "3.3")
                    return $"ROUTE_SU[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.11")
                    return $"DC_CRI[{num}]";

                if (ist == "4.10")
                    return $"SIGGROUP_CIR[{num}]";

                if (ist == "4.9")
                    return $"AVTODO_OAR[{num}]";

                if (ist == "4.8")
                    return $"AVTODO_AR[{num}]";

                if (ist == "5.11")
                    return $"ROUTE_KNO[{num}]";

                if (ist == "5.9")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "5.8")
                    return $"DC_RMU[{num}]";

                if (ist == "5.3")
                    return $"PARTROUTE_Zc[{num}]";


                throw new Exception($"SIGGROUP: unknown ist={ist}");
            }


            // =========================
            // SIGNAL_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("SIGNAL_"))
            {
                if (num == null)
                    throw new Exception($"SIGNAL: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_IS_MK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_PS_DK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_OIS_MK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_AV_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_OMO_DK[{num}:{ch}]";
                }

                if (ist == "1.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_BSV_DK[{num}:{ch}]";
                }

                if (ist == "1.6")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_OBSV_DK[{num}:{ch}]";
                }

                if (ist == "1.7")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_DPS_DK[{num}:{ch}]";
                }

                if (ist == "1.8")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_CVPS_DK[{num}:{ch}]";
                }

                if (ist == "1.9")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_CAVS_DK[{num}:{ch}]";
                }

                if (ist == "1.10")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_K_DK[{num}:{ch}]";
                }

                if (ist == "1.11")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_ORKS_MK[{num}:{ch}]";
                }

                if (ist == "1.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_RKS_MK[{num}:{ch}]";
                }

                if (ist == "1.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_PS2_DK[{num}:{ch}]";
                }

                if (ist == "1.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_DPS2_DK[{num}:{ch}]";
                }



                // OUT: 0.x
                if (ist.StartsWith("0.") & ist != "0.10")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SIGNAL_OUT_L[{num}:{ch}]";
                }

                if (ist == "0.10")
                    return $"GEN_AL[{num}]";

                if (ist == "2.15")
                    return $"SIGNAL_PSS[{num}]";

                if (ist == "2.14")
                    return $"SIGNAL_PSR[{num}]";

                if (ist == "2.13")
                    return $"SIGNAL_RO_ARS[{num}]";

                if (ist == "2.12")
                    return $"SIGNAL_SV275[{num}]";

                if (ist == "2.11")
                    return $"SIGNAL_APSO[{num}]";

                if (ist == "3.15")
                    return $"STAGE_NN[{num}]";

                if (ist == "3.14")
                    return $"STAGE_CHN[{num}]";

                if (ist == "3.13")
                    return $"OKSE_NVS[{num}]";

                if (ist == "3.12")
                    return $"STAGE_SGLS[{num}]";

                if (ist == "3.11")
                    return $"SIGNAL_PSO[{num}]";

                if (ist == "3.10")
                    return $"SIGNAL_zO[{num}]";

                if (ist == "3.9")
                    return $"SIGNAL_BO[{num}]";

                if (ist == "3.8")
                    return $"SIGNAL_SO[{num}]";

                if (ist == "3.7")
                    return $"GEN_KSAO[{num}]";

                if (ist == "3.6")
                    return $"SIGNAL_KO[{num}]";

                if (ist == "3.5")
                    return $"SIGNAL_1zHO[{num}]";

                if (ist == "3.4")
                    return $"SIGNAL_2zHO[{num}]";

                if (ist == "3.3")
                    return $"METALL_KVR[{num}]";

                if (ist == "3.2")
                    return $"SIGNAL_SKN[{num}]";

                if (ist == "4.15")
                    return $"KOD_KNOD[{num}]";

                if (ist == "4.14")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.12")
                    return $"SIGNAL_APS[{num}]";

                if (ist == "4.10")
                    return $"SIGNAL_PS[{num}]";

                if (ist == "4.9")
                    return $"SIGNAL_AVO[{num}]";

                if (ist == "4.8")
                    return $"SIGNAL_AV[{num}]";

                if (ist == "4.7")
                    return $"SIGNAL_ASS[{num}]";

                if (ist == "4.6")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "4.5")
                    return $"OKSE_L1_1[{num}]";

                if (ist == "4.4")
                    return $"SIGNAL_BSV[{num}]";

                if (ist == "4.3")
                    return $"SIGNAL_OBSV[{num}]";

                if (ist == "4.2")
                    return $"SIGNAL_OIS[{num}]";

                if (ist == "4.1")
                    return $"SIGNAL_IS[{num}]";

                if (ist == "4.0")
                    return $"SIGNAL_NLPO[{num}]";

                if (ist == "5.15")
                    return $"SIGNAL_RMSV[{num}]";

                if (ist == "5.14")
                    return $"ROUTE_KN[{num}]";

                if (ist == "5.13")
                    return $"ROUTE_MSTR[{num}]";

                if (ist == "5.12")
                    return $"OKSE_VPNR[{num}]";

                if (ist == "5.11")
                    return $"SIGNAL_CVPS[{num}]";

                if (ist == "5.10")
                    return $"KN_DMKNO[{num}]";

                if (ist == "5.9")
                    return $"DC_GOMO[{num}]";

                if (ist == "5.8")
                    return $"SIGNAL_DPS[{num}]";

                if (ist == "5.7")
                    return $"DC_CPS[{num}]";

                if (ist == "5.6")
                    return $"AVTODO_AR[{num}]";

                if (ist == "5.5")
                    return $"KOD_PNT[{num}]";

                if (ist == "5.4")
                    return $"SIGNAL_CAVS[{num}]";

                if (ist == "5.3")
                    return $"ROUTE_SU[{num}]";

                if (ist == "5.2")
                    return $"STAGE_SVP[{num}]";

                if (ist == "5.1")
                    return $"STAGE_VS[{num}]";

                if (ist == "5.0")
                    return $"SECT_P[{num}]";

                if (ist == "6.15")
                    return $"SIGNAL_VPNRS[{num}]";

                if (ist == "6.14")
                    return $"SIGNAL_DVPS[{num}]";

                if (ist == "6.13")
                    return $"OKSE_VPNO[{num}]";

                if (ist == "6.12")
                    return $"DC_DC[{num}]";

                if (ist == "6.11")
                    return $"SIGNAL_VPNOS[{num}]";

                if (ist == "6.10")
                    return $"SIGNAL_PS2[{num}]";

                if (ist == "6.8")
                    return $"SIGNAL_DPS2[{num}]";

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI4[{num}:{ch}]";
                }

                if (ist == "7.11")
                    return $"METALL_MKBV[{num}]";

                if (ist == "7.10")
                    return $"SIGNAL_RKS[{num}]";

                if (ist == "7.9")
                    return $"SIGNAL_ORKS[{num}]";

                if (ist == "7.8")
                    return $"KGU_KKGU[{num}]";

                if (ist == "7.7")
                    return $"ROUTE_OZM[{num}]";

                if (ist == "7.6")
                    return $"GEN_K275[{num}]";

                if (ist == "7.5")
                    return $"SIGNAL_OKO[{num}]";

                if (ist == "7.4")
                    return $"OKSE_RMS[{num}]";

                if (ist == "7.3")
                    return $"DC_CAV[{num}]";

                if (ist == "7.2")
                    return $"SIGNAL_PSP[{num}]";


                throw new Exception($"SIGNAL: unknown ist={ist}");
            }

            // =========================
            // CABEL_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("CABEL_"))
            {
                if (num == null)
                    throw new Exception($"CABEL: missing controller for ist={ist}");


                if (ist == "4.13")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.4")
                    return $"CABEL_PKL[{num}]";

                if (ist == "4.3")
                    return $"CABEL_KL[{num}]";

                throw new Exception($"CABEL: unknown ist={ist}");
            }


            // =========================
            // MAKET_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("MAKET_"))
            {
                if (num == null)
                    throw new Exception($"MAKET: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"MAKET_OMV_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"MAKET_MV_DK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"MAKET_MM_MK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"MAKET_MM_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"MAKET_MP_DK[{num}:{ch}]";
                }

                if (ist == "1.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"MAKET_MP_MK[{num}:{ch}]";
                }

                if (ist == "4.15")
                    return $"KOD_OD[{num}]";

                if (ist == "4.14")
                    return $"DC_DC[{num}]";

                if (ist == "4.13")
                    return $"MAKET_tMV[{num}]";

                if (ist == "4.9")
                    return $"MAKET_MP[{num}]";

                if (ist == "4.8")
                    return $"MAKET_MM[{num}]";

                if (ist == "4.7")
                    return $"KOD_OD2[{num}]";

                if (ist == "5.15")
                    return $"SWITCH_MS[{num}]";

                if (ist == "5.14")
                    return $"MAKET_OMV[{num}]";

                if (ist == "5.13")
                    return $"MAKET_MV[{num}]";

                if (ist == "5.12")
                    return $"MAKET_MPD[{num}]";

                if (ist == "5.11")
                    return $"MAKET_MMD[{num}]";

                if (ist == "5.10")
                    return $"MAKET_MPM[{num}]";

                if (ist == "5.9")
                    return $"MAKET_MMM[{num}]";

                if (ist == "5.8")
                    return $"SWITCH_MSD[{num}]";

                if (ist == "5.1")
                    return $"MAKET_t2M[{num}]";

                if (ist == "6.3")
                    return $"MAKET_MI[{num}]";


                throw new Exception($"MAKET: unknown ist={ist}");
            }


            // =========================
            // ALARMSIG_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("ALARMSIG_"))
            {
                if (num == null)
                    throw new Exception($"ALARMSIG: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ALARMSIG_ORKZ_MK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ALARMSIG_RKZ_MK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ALARMSIG_ZVL_DK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ALARMSIG_VzS_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ALARMSIG_OVzS_DK[{num}:{ch}]";
                }

                if (ist == "4.7")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.4")
                    return $"ALARMSIG_ORKZ[{num}]";

                if (ist == "4.3")
                    return $"ALARMSIG_RKZ[{num}]";

                if (ist == "4.2")
                    return $"ALARMSIG_ZVL[{num}]";

                if (ist == "4.1")
                    return $"KOD_PNT[{num}]";

                if (ist == "4.0")
                    return $"DC_CZVL[{num}]";

                if (ist == "6.5")
                    return $"ALARMSIG_OVzS[{num}]";

                if (ist == "6.4")
                    return $"ALARMSIG_VzS[{num}]";


                throw new Exception($"ALARMSIG: unknown ist={ist}");
            }

            // =========================
            // SECURITY_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("SECURITY_"))
            {
                if (num == null)
                    throw new Exception($"SECURITY: missing controller for ist={ist}");

                if (ist == "2.14")
                    return $"SECURITY_RU[{num}]";

                if (ist == "2.13")
                    return $"SECURITY_DRU[{num}]";

                if (ist == "2.6")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "2.3")
                    return $"ROUTE_SU[{num}]";

                if (ist == "2.2")
                    return $"SWITCH_MS[{num}]";

                if (ist == "2.1")
                    return $"UU_UPP[{num}]";

                if (ist == "2.0")
                    return $"SECT_P[{num}]";

                if (ist == "3.14")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.11")
                    return $"ROUTE_GS[{num}]";

                if (ist == "4.10")
                    return $"SIGNAL_KO[{num}]";

                if (ist == "4.9")
                    return $"SWITCH_MK[{num}]";

                if (ist == "4.8")
                    return $"SWITCH_PK[{num}]";

                if (ist == "4.7")
                    return $"SIGGROUP_z[{num}]";



                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECURITY_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECURITY_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECURITY_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECURITY_EI4[{num}:{ch}]";
                }

                throw new Exception($"SECURITY: unknown ist={ist}");
            }


            // =========================
            // AGP_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("AGP_"))
            {
                if (num == null)
                    throw new Exception($"AGP: missing controller for ist={ist}");

                // OUT: 0.x
                if (ist.StartsWith("0."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AGP_OUT_STATE[{num}:{ch}]";
                }

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AGP_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AGP_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AGP_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"AGP_EI4[{num}:{ch}]";
                }

                throw new Exception($"AGP: unknown ist={ist}");
            }


            // =========================
            // SPEED_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("SPEED_"))
            {
                if (num == null)
                    throw new Exception($"SPEED: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_OOS_MK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_OS70_MK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_OS60_MK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_OS40_MK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_OS0_MK[{num}:{ch}]";
                }

                if (ist == "2.15")
                    return $"GEN_ON[{num}]";

                if (ist == "2.13")
                    return $"SECURITY_RU[{num}]";

                if (ist == "2.12")
                    return $"GEN_ROG[{num}]";

                if (ist == "2.11")
                    return $"SIGNAL_VU[{num}]";

                if (ist == "2.10")
                    return $"ROUTESECT_UMP[{num}]";

                if (ist == "2.9")
                    return $"METALL_KVR[{num}]";

                if (ist == "2.8")
                    return $"KGU_KKGU[{num}]";

                if (ist == "2.6")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "2.5")
                    return $"KOD_OD2[{num}]";

                if (ist == "2.4")
                    return $"AVTOSTOP_AZ[{num}]";

                if (ist == "2.3")
                    return $"ROUTE_SU[{num}]";

                if (ist == "2.1")
                    return $"SWITCH_PK[{num}]";

                if (ist == "2.0")
                    return $"SWITCH_MK[{num}]";

                if (ist == "3.15")
                    return $"SPEED_PON[{num}]";

                if (ist == "3.14")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "3.13")
                    return $"SPEED_OS0[{num}]";

                if (ist == "3.12")
                    return $"SPEED_OOS[{num}]";

                if (ist == "3.11")
                    return $"SPEED_OS40[{num}]";

                if (ist == "3.10")
                    return $"SPEED_OS60[{num}]";

                if (ist == "3.9")
                    return $"SPEED_OS70[{num}]";

                if (ist == "3.4")
                    return $"SECT_LS[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.15")
                    return $"SPEED_SDS[{num}]";

                if (ist == "4.14")
                    return $"SPEED_FH[{num}]";

                if (ist == "4.13")
                    return $"ROUTESECT_40NUM6[{num}]";

                if (ist == "4.12")
                    return $"ROUTESECT_40NUM5[{num}]";

                if (ist == "4.11")
                    return $"ROUTESECT_40NUM4[{num}]";

                if (ist == "4.10")
                    return $"ROUTESECT_40NUM3[{num}]";

                if (ist == "4.9")
                    return $"ROUTESECT_40NUM2[{num}]";

                if (ist == "4.8")
                    return $"ROUTESECT_40NUM1[{num}]";

                if (ist == "4.7")
                    return $"SPEED_80s[{num}]";

                if (ist == "4.6")
                    return $"SPEED_70s[{num}]";

                if (ist == "4.5")
                    return $"SPEED_60s[{num}]";

                if (ist == "4.4")
                    return $"SPEED_40S[{num}]";

                if (ist == "4.3")
                    return $"SPEED_P80[{num}]";

                if (ist == "4.2")
                    return $"SPEED_P70[{num}]";

                if (ist == "4.1")
                    return $"SPEED_P60[{num}]";

                if (ist == "4.0")
                    return $"SPEED_P40[{num}]";

                if (ist == "5.15")
                    return $"STAGE_NN[{num}]";

                if (ist == "5.14")
                    return $"STAGE_CHN[{num}]";

                if (ist == "5.13")
                    return $"ROUTESECT_40CUM6[{num}]";

                if (ist == "5.12")
                    return $"ROUTESECT_40CUM5[{num}]";

                if (ist == "5.11")
                    return $"ROUTESECT_40CUM4[{num}]";

                if (ist == "5.10")
                    return $"ROUTESECT_40CUM3[{num}]";

                if (ist == "5.9")
                    return $"ROUTESECT_40CUM2[{num}]";

                if (ist == "5.8")
                    return $"ROUTESECT_40CUM1[{num}]";

                if (ist == "5.5")
                    return $"KOD_KNOD[{num}]";

                if (ist == "5.3")
                    return $"IS_KS[{num}]";

                if (ist == "7.9")
                    return $"SPEED_80U[{num}]";

                if (ist == "7.8")
                    return $"SPEED_70U[{num}]";

                if (ist == "7.7")
                    return $"SPEED_60U[{num}]";

                if (ist == "7.6")
                    return $"SPEED_40U[{num}]";

                if (ist == "7.5")
                    return $"ROUTESECT_40NUM[{num}]";

                if (ist == "7.4")
                    return $"ROUTESECT_40CUM[{num}]";

                if (ist == "7.1")
                    return $"SECT_MSP[{num}]";

                if (ist == "7.0")
                    return $"SECT_PN[{num}]";


                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SPEED_EI4[{num}:{ch}]";
                }

                throw new Exception($"SPEED: unknown ist={ist}");
            }


            // =========================
            // GEN_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("GEN_"))
            {
                if (num == null)
                    throw new Exception($"GEN: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_ORO_MK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_RO_MK[{num}:{ch}]";
                }

                // OUT: 0.x
                if (ist.StartsWith("0.") & (ist != "0.7" | ist != "0.6" | ist != "0.5"| ist != "0.4"| ist != "0.3"| ist != "0.2"))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_OUT[{num}:{ch}]";
                }

                if (ist == "0.7")
                    return $"GEN_PFR2[{num}]";

                if (ist == "0.6")
                    return $"GEN_PFR1[{num}]";

                if (ist == "0.5")
                    return $"GEN_PFR0[{num}]";

                if (ist == "0.4")
                    return $"GEN_FR2[{num}]";

                if (ist == "0.3")
                    return $"GEN_FR1[{num}]";

                if (ist == "0.2")
                    return $"GEN_FR0[{num}]";

                if (ist == "2.12")
                    return $"GEN_ORO[{num}]";

                if (ist == "2.11")
                    return $"GEN_ROG[{num}]";

                if (ist == "2.10")
                    return $"GEN_ON[{num}]";

                if (ist == "2.9")
                    return $"ROUTESECT_UMP[{num}]";

                if (ist == "2.8")
                    return $"GEN_OTK[{num}]";

                if (ist == "2.6")
                    return $"SIGNAL_ASS[{num}]";

                if (ist == "2.5")
                    return $"SIGNAL_SVOD[{num}]";

                if (ist == "2.4")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "2.3")
                    return $"GEN_ROPP[{num}]";

                if (ist == "2.2")
                    return $"KOD_OD2[{num}]";

                if (ist == "2.1")
                    return $"ROUTE_SU[{num}]";

                if (ist == "2.0")
                    return $"GEN_ROPS[{num}]";

                if (ist == "3.2")
                    return $"SECT_P[{num}]";

                if (ist == "4.13")
                    return $"ROUTESECT_CHUMOS[{num}]";

                if (ist == "4.12")
                    return $"ROUTESECT_NUMOS[{num}]";

                if (ist == "4.11")
                    return $"STAGE_OKL[{num}]";

                if (ist == "4.10")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.9")
                    return $"SECT_PN[{num}]";

                if (ist == "4.8")
                    return $"ROUTESECT_40CUM[{num}]";

                if (ist == "4.7")
                    return $"ROUTESECT_40NUM[{num}]";

                if (ist == "4.6")
                    return $"SPEED_80U[{num}]";

                if (ist == "4.5")
                    return $"SPEED_70U[{num}]";

                if (ist == "4.4")
                    return $"SPEED_60U[{num}]";

                if (ist == "4.3")
                    return $"SPEED_40U[{num}]";

                if (ist == "4.2")
                    return $"SPEED_CHRS[{num}]";

                if (ist == "4.1")
                    return $"SPEED_FH[{num}]";

                if (ist == "4.0")
                    return $"SPEED_NRS[{num}]";

                if (ist == "5.15")
                    return $"GEN_FR2[{num}]";

                if (ist == "5.14")
                    return $"GEN_FR1[{num}]";

                if (ist == "5.13")
                    return $"GEN_FR0[{num}]";

                if (ist == "5.12")
                    return $"GEN_PFR2[{num}]";

                if (ist == "5.11")
                    return $"GEN_PFR1[{num}]";

                if (ist == "5.10")
                    return $"GEN_PFR0[{num}]";

                if (ist == "5.6")
                    return $"GEN_C[{num}]";

                if (ist == "5.5")
                    return $"GEN_NKG2[{num}]";

                if (ist == "5.4")
                    return $"GEN_CPnk[{num}]";

                if (ist == "5.3")
                    return $"STAGE_CHN[{num}]";

                if (ist == "5.2")
                    return $"GEN_AOG2[{num}]";

                if (ist == "5.1")
                    return $"GEN_c0[{num}]";

                if (ist == "6.6")
                    return $"GEN_N[{num}]";

                if (ist == "6.5")
                    return $"GEN_NKG1[{num}]";

                if (ist == "6.4")
                    return $"GEN_NPnk[{num}]";

                if (ist == "6.3")
                    return $"STAGE_NN[{num}]";

                if (ist == "6.2")
                    return $"GEN_AOG1[{num}]";

                if (ist == "6.1")
                    return $"GEN_n0[{num}]";

                if (ist == "7.14")
                    return $"GEN_AL[{num}]";

                if (ist == "7.13")
                    return $"GEN_UR_K[{num}]";

                if (ist == "7.12")
                    return $"GEN_P275K[{num}]";

                if (ist == "7.11")
                    return $"GEN_P225K[{num}]";

                if (ist == "7.10")
                    return $"GEN_P175K[{num}]";

                if (ist == "7.9")
                    return $"GEN_P125K[{num}]";

                if (ist == "7.8")
                    return $"GEN_P325K[{num}]";

                if (ist == "7.7")
                    return $"GEN_SAOK[{num}]";

                if (ist == "7.6")
                    return $"GEN_O275K[{num}]";

                if (ist == "7.5")
                    return $"GEN_O225K[{num}]";

                if (ist == "7.4")
                    return $"GEN_O175K[{num}]";

                if (ist == "7.3")
                    return $"GEN_O125K[{num}]";

                if (ist == "7.2")
                    return $"GEN_O75K[{num}]";

                if (ist == "7.1")
                    return $"GEN_UR1_K[{num}]";

                if (ist == "7.0")
                    return $"GEN_UR2_K[{num}]";


                if (ist == "3.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_EI1[{num}:{ch}]";
                }

                if (ist == "3.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_EI2[{num}:{ch}]";
                }

                if (ist == "3.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_EI3[{num}:{ch}]";
                }

                if (ist == "3.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_EI4[{num}:{ch}]";
                }



                throw new Exception($"GEN: unknown ist={ist}");
            }


            // =========================
            // OKSE_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("OKSE_"))
            {
                if (num == null)
                    throw new Exception($"OKSE: missing controller for ist={ist}");



                if (ist.StartsWith("0.") & (ist != "0.9" | ist != "0.8" | ist != "0.7" | ist != "0.6" | ist != "0.5" | ist != "0.4" | ist != "0.3" | ist != "0.2" | ist != "0.1" | ist != "0.0"))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"GEN_OUT[{num}:{ch}]";
                }
                // OUT: 0.x
                if (ist == "0.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L5_0[{num}:{ch}]";
                }

                if (ist == "0.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L5_1[{num}:{ch}]";
                }

                if (ist == "0.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L4_0[{num}:{ch}]";
                }

                if (ist == "0.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L4_1[{num}:{ch}]";
                }

                if (ist == "0.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L3_0[{num}:{ch}]";
                }

                if (ist == "0.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L3_1[{num}:{ch}]";
                }

                if (ist == "0.6")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L2_0[{num}:{ch}]";
                }

                if (ist == "0.7")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L2_1[{num}:{ch}]";
                }

                if (ist == "0.8")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L1_0[{num}:{ch}]";
                }

                if (ist == "0.9")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"OKSE_L1_1[{num}:{ch}]";
                }

                if (ist == "3.15")
                    return $"AVTOSTOP_AZ[{num}]";

                if (ist == "3.14")
                    return $"KGU_OKGU[{num}]";

                if (ist == "3.13")
                    return $"STAGE_OKL[{num}]";

                if (ist == "3.12")
                    return $"AVTOSTOP_AZ[{num}]";

                if (ist == "3.11")
                    return $"ROUTE_OPS[{num}]";

                if (ist == "3.10")
                    return $"METALL_KVR[{num}]";

                if (ist == "3.9")
                    return $"ROUTE_MUI[{num}]";

                if (ist == "3.8")
                    return $"ROUTE_1ZHLVNR[{num}]";

                if (ist == "3.7")
                    return $"ROUTE_1ZHLVMR[{num}]";

                if (ist == "3.6")
                    return $"ROUTE_SLv[{num}]";

                if (ist == "3.5")
                    return $"ROUTE_BLv[{num}]";

                if (ist == "3.4")
                    return $"ROUTE_zLv[{num}]";

                if (ist == "3.3")
                    return $"ROUTE_2ZHLv[{num}]";

                if (ist == "3.2")
                    return $"ROUTE_KLv[{num}]";

                if (ist == "4.12")
                    return $"SIGNAL_IS[{num}]";

                if (ist == "4.11")
                    return $"OKSE_NVS[{num}]";

                if (ist == "4.10")
                    return $"OKSE_RMS[{num}]";

                if (ist == "4.9")
                    return $"OKSE_RMK[{num}]";

                if (ist == "4.8")
                    return $"OKSE_RMS2[{num}]";

                if (ist == "4.7")
                    return $"ROUTEPOINTER_RKMU[{num}]";

                if (ist == "4.6")
                    return $"OKSE_OKRK[{num}]";

                if (ist == "4.5")
                    return $"SIGNAL_RKS[{num}]";

                if (ist == "4.4")
                    return $"ALARMSIG_RKZ[{num}]";

                if (ist == "4.3")
                    return $"ALARMSIG_VzS[{num}]";

                if (ist == "4.2")
                    return $"ALARMSIG_ZVL[{num}]";

                if (ist == "4.1")
                    return $"PARTROUTE_UVzVch[{num}]";

                if (ist == "4.0")
                    return $"SIGNAL_PSS[{num}]";




                throw new Exception($"OKSE: unknown ist={ist}");
            }


            // =========================
            // KOD_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("KOD_"))
            {
                if (num == null)
                    throw new Exception($"BELL: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KOD_OOD_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KOD_POD_DK[{num}:{ch}]";
                }

                if (ist == "4.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"KOD_LNK[{num}:{ch}]";
                }

                if (ist == "4.15")
                    return $"KOD_OD[{num}]";

                if (ist == "4.14")
                    return $"KOD_OOD[{num}]";

                if (ist == "4.13")
                    return $"KOD_tOD[{num}]";

                if (ist == "4.12")
                    return $"SIGNAL_SVOD2[{num}]";

                if (ist == "4.11")
                    return $"SIGNAL_SVOD[{num}]";

                if (ist == "4.10")
                    return $"DC_DC[{num}]";

                if (ist == "4.9")
                    return $"KOD_KNOD[{num}]";

                if (ist == "4.8")
                    return $"DC_CRI[{num}]";

                if (ist == "4.7")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.6")
                    return $"STAGE_ANK[{num}]";

                if (ist == "4.5")
                    return $"STAGE_ACHK[{num}]";

                if (ist == "5.15")
                    return $"MAKET_MIV[{num}]";

                if (ist == "5.13")
                    return $"MAKET_MV[{num}]";

                if (ist == "6.8")
                    return $"GEN_K40RS[{num}]";

                if (ist == "6.7")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "6.6")
                    return $"SECT_PN[{num}]";

                if (ist == "6.5")
                    return $"ALARMSIG_ZVL[{num}]";

                if (ist == "6.4")
                    return $"BELL_KZV[{num}]";

                if (ist == "6.3")
                    return $"MAKET_MI[{num}]";

                if (ist == "6.2")
                    return $"KURBEL_NVK[{num}]";

                if (ist == "6.1")
                    return $"KURBEL_KDI[{num}]";

                if (ist == "6.0")
                    return $"KOD_POD[{num}]";


                throw new Exception($"KOD: unknown ist={ist}");
            }


            // =========================
            // DC_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("DC_"))
            {
                if (num == null)
                    throw new Exception($"DC: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_CRI_DK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_DU_DK[{num}:{ch}]";
                }

                if (ist == "1.2")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_MU_DK[{num}:{ch}]";
                }

                if (ist == "1.3")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_CAN_DK[{num}:{ch}]";
                }

                if (ist == "1.4")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_CZVL_DK[{num}:{ch}]";
                }

                if (ist == "1.5")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_GOMO_DK[{num}:{ch}]";
                }

                if (ist == "1.6")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_GO_DK[{num}:{ch}]";
                }

                if (ist == "1.7")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_CAV_DK[{num}:{ch}]";
                }

                if (ist == "1.8")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"DC_CPS_DK[{num}:{ch}]";
                }

                if (ist == "4.15")
                    return $"DC_DC[{num}]";

                if (ist == "4.14")
                    return $"DC_KDC[{num}]";

                if (ist == "4.13")
                    return $"DC_RMU[{num}]";

                if (ist == "4.12")
                    return $"DC_KRMU[{num}]";

                if (ist == "4.11")
                    return $"DC_GOMO[{num}]";

                if (ist == "4.10")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.9")
                    return $"KOD_KNOD[{num}]";

                if (ist == "4.8")
                    return $"SIGNAL_SOK[{num}]";

                if (ist == "4.7")
                    return $"SWITCH_NKS[{num}]";

                if (ist == "4.6")
                    return $"SWITCH_RK[{num}]";

                if (ist == "4.5")
                    return $"DC_CPS[{num}]";

                if (ist == "4.4")
                    return $"DC_CRI[{num}]";

                if (ist == "4.2")
                    return $"DC_CZVL[{num}]";

                if (ist == "4.1")
                    return $"DC_GROM[{num}]";

                if (ist == "4.0")
                    return $"SIGNAL_OMO[{num}]";

                if (ist == "5.13")
                    return $"SECT_CAVN[{num}]";

                if (ist == "5.12")
                    return $"SIGNAL_DPS[{num}]";

                if (ist == "5.11")
                    return $"SIGNAL_CAVS[{num}]";

                if (ist == "5.10")
                    return $"DC_tDC3[{num}]";

                if (ist == "5.9")
                    return $"DC_CAV[{num}]";

                if (ist == "5.8")
                    return $"DC_CAN[{num}]";

                if (ist == "5.7")
                    return $"DC_NDC[{num}]";

                if (ist == "5.6")
                    return $"MAKET_MV[{num}]";

                if (ist == "5.5")
                    return $"DC_tDC2[{num}]";

                if (ist == "5.4")
                    return $"DC_GKNO[{num}]";

                if (ist == "5.3")
                    return $"DC_GV[{num}]";

                if (ist == "5.2")
                    return $"KN_DKNO[{num}]";

                if (ist == "5.1")
                    return $"KURBEL_KKUD[{num}]";

                if (ist == "5.0")
                    return $"DC_tDC[{num}]";

                if (ist == "6.7")
                    return $"RELAY_KRK1[{num}]";

                throw new Exception($"DC: unknown ist={ist}");
            }


            // =========================
            // KN_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("KN_"))
            {
                if (num == null)
                    throw new Exception($"KN: missing controller for ist={ist}");

                if (ist == "3.14")
                    return $"ROUTE_MSTR[{num}]";

                if (ist == "3.12")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "3.10")
                    return $"SIGGROUP_IA[{num}]";

                if (ist == "3.8")
                    return $"ROUTE_PP[{num}]";

                if (ist == "4.14")
                    return $"ROUTE_AN[{num}]";

                if (ist == "4.13")
                    return $"KN_DKNO[{num}]";

                if (ist == "4.12")
                    return $"KN_DKN[{num}]";

                if (ist == "4.11")
                    return $"KN_NKN[{num}]";

                if (ist == "4.10")
                    return $"KN_AKN[{num}]";

                if (ist == "4.9")
                    return $"KN_VKN[{num}]";

                if (ist == "4.8")
                    return $"AVTODO_AR[{num}]";

                if (ist == "4.7")
                    return $"AVTODO_OAR[{num}]";

                if (ist == "4.2")
                    return $"ROUTE_KN[{num}]";

                if (ist == "4.1")
                    return $"ROUTE_KNO[{num}]";

                if (ist == "5.15")
                    return $"DC_GKNO[{num}]";

                if (ist == "5.14")
                    return $"DC_GOMO[{num}]";

                if (ist == "5.13")
                    return $"SIGNAL_DK[{num}]";

                if (ist == "5.12")
                    return $"KN_DMKNO[{num}]";

                if (ist == "5.9")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "5.3")
                    return $"PARTROUTE_Zc[{num}]";

                if (ist == "5.2")
                    return $"SWITCH_PK[{num}]";

                if (ist == "5.1")
                    return $"SWITCH_MK[{num}]";

                if (ist == "5.0")
                    return $"PARTROUTE_STRch[{num}]";


                throw new Exception($"KN: unknown ist={ist}");
            }


            // =========================
            // BELL_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("UU_"))
            {
                if (num == null)
                    throw new Exception($"UU: missing controller for ist={ist}");

                if (ist == "4.13")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.0")
                    return $"SECT_P[{num}]";

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"UU_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"UU_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"UU_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"UU_EI4[{num}:{ch}]";
                }

                throw new Exception($"UU: unknown ist={ist}");
            }

            // =========================
            // ROUTEPOINTER_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("ROUTEPOINTER_"))
            {
                if (num == null)
                    throw new Exception($"ROUTEPOINTER: missing controller for ist={ist}");
                if (ist == "1.0")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ROUTEPOINTER_ORKMU_MK[{num}:{ch}]";
                }

                if (ist == "1.1")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ROUTEPOINTER_RKMU_MK[{num}:{ch}]";
                }

                // OUT: 0.x
                if (ist.StartsWith("0."))
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"ROUTEPOINTER_OUT[{num}:{ch}]";
                }

                if (ist == "7.15")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI1[{num}:{ch}]";
                }

                if (ist == "7.14")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI2[{num}:{ch}]";
                }

                if (ist == "7.13")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI3[{num}:{ch}]";
                }

                if (ist == "7.12")
                {
                    var ch = int.Parse(ist.Substring(2));
                    return $"SECT_EI4[{num}:{ch}]";
                }


                if (ist == "4.13")
                    return $"KOD_OD2[{num}]";

                if (ist == "4.1")
                    return $"OKSE_VPNO[{num}]";

                if (ist == "4.0")
                    return $"OKSE_VPNR[{num}]";

                if (ist == "5.1")
                    return $"ROUTEPOINTER_VPNOM[{num}]";

                if (ist == "5.0")
                    return $"ROUTEPOINTER_VPNRM[{num}]";

                if (ist == "6.10")
                    return $"OKSE_RMS[{num}]";

                if (ist == "6.9")
                    return $"ROUTEPOINTER_RMSM[{num}]";

                if (ist == "7.10")
                    return $"ROUTEPOINTER_RKMU[{num}]";

                if (ist == "7.9")
                    return $"ROUTEPOINTER_ORKMU[{num}]";

                throw new Exception($"ROUTEPOINTER: unknown ist={ist}");
            }

            // =========================
            // BELL_*  (OUT)
            // =========================
            if (targetSheet.StartsWith("BELL_"))
            {
                if (num == null)
                    throw new Exception($"BELL: missing controller for ist={ist}");

                if (ist == "2.3")
                    return $"STAGE_CHN[{num}]";

                if (ist == "2.2")
                    return $"STAGE_CHK[{num}]";

                if (ist == "2.1")
                    return $"STAGE_ACHK[{num}]";

                if (ist == "2.0")
                    return $"STAGE_NN[{num}]";

                if (ist == "3.15")
                    return $"STAGE_NK[{num}]";

                if (ist == "3.14")
                    return $"STAGE_ANK[{num}]";

                if (ist == "3.13")
                    return $"STAGE_SVPM[{num}]";

                if (ist == "3.12")
                    return $"SIGGROUP_TSKNO[{num}]";

                if (ist == "3.11")
                    return $"SIGGROUP_TSPP[{num}]";

                if (ist == "3.10")
                    return $"SIGGROUP_TSGS[{num}]";

                if (ist == "3.9")
                    return $"SIGGROUP_TSM2[{num}]";

                if (ist == "3.8")
                    return $"SIGGROUP_TSM1[{num}]";

                if (ist == "3.7")
                    return $"SIGGROUP_ISK[{num}]";

                if (ist == "3.6")
                    return $"STAGE_DS[{num}]";

                if (ist == "3.5")
                    return $"STAGE_ZS[{num}]";

                if (ist == "3.4")
                    return $"SIGNAL_OKO[{num}]";

                if (ist == "3.3")
                    return $"SECT_zD45[{num}]";

                if (ist == "3.2")
                    return $"SECT_zD13[{num}]";

                if (ist == "4.15")
                    return $"OKSE_RMS[{num}]";

                if (ist == "4.14")
                    return $"PARTROUTE_Zc[{num}]";

                if (ist == "4.13")
                    return $"GEN_K275[{num}]";

                if (ist == "4.12")
                    return $"GEN_KSAO[{num}]";

                if (ist == "4.11")
                    return $"SIGNAL_SVOD[{num}]";

                if (ist == "4.10")
                    return $"SECURITY_OHR[{num}]";

                if (ist == "4.9")
                    return $"SECURITY_RU[{num}]";

                if (ist == "4.8")
                    return $"SPEED_CHRS[{num}]";

                if (ist == "4.7")
                    return $"SPEED_NRS[{num}]";

                if (ist == "4.6")
                    return $"RELAY_KRK1[{num}]";

                if (ist == "4.5")
                    return $"SIGNAL_RO_ARS[{num}]";

                if (ist == "4.4")
                    return $"SIGNAL_RO_AB[{num}]";

                if (ist == "4.3")
                    return $"STAGE_VS[{num}]";

                if (ist == "4.2")
                    return $"SPEED_HV[{num}]";

                if (ist == "4.1")
                    return $"SECT_Lz[{num}]";

                if (ist == "4.0")
                    return $"SECT_LS[{num}]";

                if (ist == "5.15")
                    return $"GEN_ON[{num}]";

                if (ist == "5.14")
                    return $"GEN_ROG[{num}]";

                if (ist == "5.13")
                    return $"GEN_C[{num}]";

                if (ist == "5.12")
                    return $"GEN_N[{num}]";

                if (ist == "5.11")
                    return $"SPEED_OS70[{num}]";

                if (ist == "5.10")
                    return $"SPEED_OS60[{num}]";

                if (ist == "5.9")
                    return $"SPEED_OS40[{num}]";

                if (ist == "5.8")
                    return $"SPEED_OS0[{num}]";

                if (ist == "5.7")
                    return $"SPEED_SDS[{num}]";

                if (ist == "5.6")
                    return $"SPEED_FH[{num}]";

                if (ist == "5.5")
                    return $"SPEED_80U[{num}]";

                if (ist == "5.4")
                    return $"SPEED_70U[{num}]";

                if (ist == "5.3")
                    return $"SPEED_60U[{num}]";

                if (ist == "5.2")
                    return $"SPEED_40U[{num}]";

                if (ist == "5.1")
                    return $"ROUTESECT_40CUM[{num}]";

                if (ist == "5.0")
                    return $"ROUTESECT_40NUM[{num}]";

                if (ist == "6.15")
                    return $"SIGNAL_VU[{num}]";

                if (ist == "6.14")
                    return $"SIGNAL_BSV[{num}]";

                if (ist == "6.13")
                    return $"SIGNAL_IS[{num}]";

                if (ist == "6.12")
                    return $"SIGNAL_ASS[{num}]";

                if (ist == "6.11")
                    return $"SIGNAL_AV[{num}]";

                if (ist == "6.10")
                    return $"SIGNAL_PSO[{num}]";

                if (ist == "6.9")
                    return $"SIGNAL_1zHO[{num}]";

                if (ist == "6.8")
                    return $"SIGNAL_2zHO[{num}]";

                if (ist == "6.7")
                    return $"SIGNAL_BO[{num}]";

                if (ist == "6.6")
                    return $"SIGNAL_SO[{num}]";

                if (ist == "6.5")
                    return $"SIGNAL_zO[{num}]";

                if (ist == "6.4")
                    return $"SIGNAL_KO[{num}]";

                if (ist == "6.3")
                    return $"SIGGROUP_z[{num}]";

                if (ist == "6.2")
                    return $"PARTROUTE_UCH[{num}]";

                if (ist == "6.1")
                    return $"ROUTE_S[{num}]";

                if (ist == "6.0")
                    return $"ROUTE_SU[{num}]";

                if (ist == "7.15")
                    return $"STAGE_BLP[{num}]";

                if (ist == "7.14")
                    return $"SWITCH_MK[{num}]";

                if (ist == "7.13")
                    return $"SWITCH_PK[{num}]";

                if (ist == "7.12")
                    return $"KURBEL_KKUK[{num}]";

                if (ist == "7.11")
                    return $"STAGE_SVP[{num}]";

                if (ist == "7.10")
                    return $"STAGE_OP_U[{num}]";

                if (ist == "7.9")
                    return $"ROUTE_vS[{num}]";

                if (ist == "7.8")
                    return $"METALL_MKBV[{num}]";

                if (ist == "7.7")
                    return $"METALL_MKR[{num}]";

                if (ist == "7.6")
                    return $"METALL_MKKV[{num}]";

                if (ist == "7.5")
                    return $"AVTOSTOP_VA[{num}]";

                if (ist == "7.4")
                    return $"KGU_OKGU[{num}]";

                if (ist == "7.3")
                    return $"SECT_BRC[{num}]";

                if (ist == "7.2")
                    return $"SECT_zD125[{num}]";

                if (ist == "7.1")
                    return $"SECT_P[{num}]";

                if (ist == "7.0")
                    return $"AGP_NAGP[{num}]";

                throw new Exception($"BELL: unknown ist={ist}");
            }
            throw new Exception(
                $"No SourceRule: sheet={targetSheet}, ist={ist}, num={num}");
        }
    }
}
