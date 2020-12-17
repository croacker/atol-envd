using Atol.Drivers10.Fptr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace atol_reg
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStartClick(object sender, EventArgs e)
        {
            start();
        }

        private void start()
        {

            var fptrCommon = new Fptr();
            fptrCommon.setSingleSetting(Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO.ToString());            // Задаем автоматическое определение модели ККМ
            //fptrCommon .setSingleSetting(Constants.LIBFPTR_SETTING_PORT, (KKM10.LIBFPTR_PORT_COM));                    // Задаем подключение по COM-порту
            fptrCommon.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_USB.ToString());                    // Задаем подключение по USB
            //fptrCommon .setSingleSetting(Constants.LIBFPTR_SETTING_COM_FILE, "COM1");                                // Задаем номер COM-порта
            //fptrCommon .setSingleSetting(Constants.LIBFPTR_SETTING_BAUDRATE, Constants.LIBFPTR_PORT_BR_115200.ToString());        // Задаем скорость обмена

            fptrCommon.applySingleSettings();                                                                    // Применяем настройки
            fptrCommon.open();

            if (fptrCommon.errorCode() != 0)
            {
                var description = fptrCommon.errorDescription();
                lbLog.Items.Add(description);
                MessageBox.Show(description);                                        // Если есть ошибки при открытии соед-я, то выводим сообщение с описанием ошибки
                return;                                                                                        // и выходим
            }

            // == Считываем параметры рег-ии ==
            fptrCommon.setParam(Constants.LIBFPTR_PARAM_FN_DATA_TYPE, Constants.LIBFPTR_FNDT_REG_INFO);
            fptrCommon.fnQueryData();

            var fptrParameters = new FptrParameters();
            fptrParameters.TaxationTypes = fptrCommon.getParamInt(1062);                                            // Текущая СНО
            fptrParameters.AgentSign = fptrCommon.getParamInt(1057);
            fptrParameters.FfdVersion = fptrCommon.getParamInt(1209);
            fptrParameters.AutoModeSign = fptrCommon.getParamBool(1001);
            fptrParameters.OfflineModeSign = fptrCommon.getParamBool(1002);
            fptrParameters.EncryptionSign = fptrCommon.getParamBool(1056);
            fptrParameters.InternetSign = fptrCommon.getParamBool(1108);
            fptrParameters.ServiceSign = fptrCommon.getParamBool(1109);
            fptrParameters.BsoSign = fptrCommon.getParamBool(1110);
            fptrParameters.LotterySign = fptrCommon.getParamBool(1126);
            fptrParameters.GamblingSign = fptrCommon.getParamBool(1193);
            fptrParameters.ExciseSign = fptrCommon.getParamBool(1207);
            fptrParameters.MachineInstallationSign = fptrCommon.getParamBool(1221);
            fptrParameters.FnsUrl = fptrCommon.getParamString(1060);
            fptrParameters.OrganizationName = fptrCommon.getParamString(1048);
            fptrParameters.OrganizationEmail = fptrCommon.getParamString(1117);
            fptrParameters.PaymentsAddressM = fptrCommon.getParamString(1187);
            fptrParameters.PaymentsAddress = fptrCommon.getParamString(1009);
            fptrParameters.MachineNumber = fptrCommon.getParamString(1036);
            fptrParameters.OfdVATIN = fptrCommon.getParamString(1017);
            fptrParameters.OfdName = fptrCommon.getParamString(1046);
            // == Закончили считывать параметры рег-ии ==

            if ((fptrParameters.TaxationTypes == 8) && (GetTime() > 1609459200))                // Если текущая СНО = ЕНВД и текущая дата больше 01.01.2021, то...
            {
                reg(fptrCommon, fptrParameters);// ... перерегистрация
                var description = "Перерегистрация успешно проведена!";
                lbLog.Items.Add(description);
                MessageBox.Show(description);                            // Сообщение
            }
            fptrCommon.close();
        }


         private void reg(Fptr fptrCommon, FptrParameters fptrParameters)
        {

            fptrCommon.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_SHIFT_STATE);                    // Запрашиваем состояние смены
            fptrCommon.queryData();                                                                                // Всё еще запрашиваем
            var state = fptrCommon.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE);                                        // Продолжаем запрашивать

            if (state != 0)                                                                                    // Если смена НЕ закрыта, то...
            {
                fptrCommon.close();
                MessageBox.Show("Смена открыта. Закройте смену и перезапустите Frontol");            // ... ругаемся
                return;
                // и выходим
            }

            fptrCommon.setParam(Constants.LIBFPTR_PARAM_FN_OPERATION_TYPE, Constants.LIBFPTR_FNOP_CHANGE_PARAMETERS);
            fptrCommon.setParam(1060, fptrParameters.FnsUrl);
            fptrCommon.setParam(1009, fptrParameters.PaymentsAddress);
            fptrCommon.setParam(1048, fptrParameters.OrganizationName);
            fptrCommon.setParam(1117, fptrParameters.OrganizationEmail);
            fptrCommon.setParam(1057, fptrParameters.AgentSign);
            fptrCommon.setParam(1187, fptrParameters.PaymentsAddressM);
            fptrCommon.setParam(1209, fptrParameters.FfdVersion);
            fptrCommon.setParam(1001, fptrParameters.AutoModeSign);
            fptrCommon.setParam(1036, fptrParameters.MachineNumber);
            fptrCommon.setParam(1002, fptrParameters.OfflineModeSign);
            fptrCommon.setParam(1056, fptrParameters.EncryptionSign);
            fptrCommon.setParam(1108, fptrParameters.InternetSign);
            fptrCommon.setParam(1109, fptrParameters.ServiceSign);
            fptrCommon.setParam(1110, fptrParameters.BsoSign);
            fptrCommon.setParam(1126, fptrParameters.LotterySign);
            fptrCommon.setParam(1193, fptrParameters.GamblingSign);
            fptrCommon.setParam(1207, fptrParameters.ExciseSign);
            fptrCommon.setParam(1221, fptrParameters.MachineInstallationSign);
            fptrCommon.setParam(1017, fptrParameters.OfdVATIN);
            fptrCommon.setParam(1046, fptrParameters.OfdName);
            fptrCommon.setParam(1101, 4);


            // Далее раскомментировать нужную строку со своей новой СНО
            fptrCommon.setParam(1062, Constants.LIBFPTR_TT_USN_INCOME_OUTCOME);            // УСН Д-Р
                                                                                       //KKM10.setParam(1062, KKM10.LIBFPTR_TT_USN_INCOME);                // УСН Д    
                                                                                       //KKM10.setParam(1062, KKM10.LIBFPTR_TT_OSN);                        // ОСН
                                                                                       //KKM10.setParam(1062, KKM10.LIBFPTR_TT_PATENT );                    // Патент
                                                                                       //KKM10.setParam(1062, KKM10.LIBFPTR_TT_ESN);                        // ЕСХН

            fptrCommon.fnOperation();

            if (fptrCommon.errorCode() != 0)                                                    // Проверяем ошибки
            {
                var description = fptrCommon.errorDescription();
                lbLog.Items.Add(description);
                MessageBox.Show(description); // Если есть ошибки, то выводим сообщение с описанием ошибки
                fptrCommon.close();
                return;
            }

            fptrCommon.setParam(Constants.LIBFPTR_PARAM_SETTING_ID, 50);                            // Устанавливаем СНО по умолчанию
            fptrCommon.setParam(Constants.LIBFPTR_PARAM_SETTING_VALUE, '4');                        // Тут выбираем СНО: 1 - ОСН, 2 - УСН (Д), 4 - УСН (Д-Р), 16 - ЕСХН, 32 - Патент
            fptrCommon.writeDeviceSetting();                                                    // Заканчиваем устанавливать

            if (fptrCommon.errorCode() != 0)                                                    // Проверяем ошибки
            {
                var description = fptrCommon.errorDescription();
                lbLog.Items.Add(description);
                MessageBox.Show(description);                     // Если есть ошибки, то выводим сообщение с описанием ошибки
                fptrCommon.close();
                return;
            }
        }

        //Получить дату, время текущее.
        private int GetTime()
        {            
            var time = (dtpReg.Value.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (int)(time.TotalMilliseconds/1000 + 0.5);
        }

    }

 

}
