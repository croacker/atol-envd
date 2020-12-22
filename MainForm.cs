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

        private void btnCheckConnection_Click(object sender, EventArgs e)
        {
            checkConnection();
        }

        private void btnStartClick(object sender, EventArgs e)
        {
            start();
        }

        /// <summary>
        /// Проверить соединение
        /// </summary>
        private void checkConnection()
        {
            var fptrCommon = connect();
            if (fptrCommon.errorCode() != 0)
            {
                var msg = fptrCommon.errorDescription();
                addLog(msg);
                showError(msg); // Если есть ошибки при открытии соед-я, то выводим сообщение с описанием ошибки
                fptrCommon.close();
                return;                                                                                        // и выходим
            }

            // == Считываем параметры рег-ии ==            
            var fptrParameters = getFptrParameters(fptrCommon);
            addLog("Система налогообложения(1062):" + fptrParameters.TaxationTypes.ToString());
            addLog("Признак агента(1057):" + fptrParameters.AgentSign.ToString());
            addLog("Версия ФФД(1209):" + fptrParameters.FfdVersion.ToString());
            addLog("Признак автоматического режима(1001):" + fptrParameters.AutoModeSign.ToString());
            addLog("Признак автономного режима(1002):" + fptrParameters.OfflineModeSign.ToString());
            addLog("Признак шифрования(1056):" + fptrParameters.EncryptionSign.ToString());
            addLog("Признак ККТ для расчетов в сети Интернет(1108):" + fptrParameters.InternetSign.ToString());
            addLog("Признак расчетов за услуги(1109):" + fptrParameters.ServiceSign.ToString());
            addLog("Признак АС БСО(1110):" + fptrParameters.BsoSign.ToString());
            addLog("Признак проведения лотерей(1126):" + fptrParameters.LotterySign.ToString());
            addLog("Признак проведения азартных игр(1193):" + fptrParameters.GamblingSign.ToString());
            addLog("Признак подакцизного товара(1207):" + fptrParameters.ExciseSign.ToString());
            addLog("Признак установки в автомате(1221):" + fptrParameters.MachineInstallationSign.ToString());
            addLog("Адрес сайта ФНС(1060):" + fptrParameters.FnsUrl);
            addLog("Название организации(1048):" + fptrParameters.OrganizationName);
            addLog("E-mail организации(1117):" + fptrParameters.OrganizationEmail);
            addLog("Адрес места расчетов(1187):" + fptrParameters.PaymentsAddressM);
            addLog("Адрес расчетов(1009):" + fptrParameters.PaymentsAddress);
            addLog("Номер автомата(1036):" + fptrParameters.MachineNumber);
            addLog("ИНН ОФД(1017):" + fptrParameters.OfdVATIN);
            addLog("Название ОФД(1046):" + fptrParameters.OfdName);
            fptrCommon.close();
        }


        private void start()
        {

            var fptrCommon = connect();
            if (fptrCommon.errorCode() != 0)
            {
                var msg = fptrCommon.errorDescription();
                addLog(msg);
                showError(msg);                                        // Если есть ошибки при открытии соед-я, то выводим сообщение с описанием ошибки
                return;                                                                                        // и выходим
            }

            // == Считываем параметры рег-ии ==
            var fptrParameters = getFptrParameters(fptrCommon);
            // == Закончили считывать параметры рег-ии ==

            if ((fptrParameters.TaxationTypes == 8) && (GetTime() > 1609459200))                // Если текущая СНО = ЕНВД и текущая дата больше 01.01.2021, то...
            {
                reg(fptrCommon, fptrParameters);// ... перерегистрация
                var msg = "Перерегистрация успешно проведена!";
                addLog(msg);
                MessageBox.Show(msg);                            // Сообщение
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
                var msg = "Смена открыта. Закройте смену и перезапустите Frontol";
                addLog(msg);
                showError(msg);            // ... ругаемся
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
                var msg = fptrCommon.errorDescription();
                addLog(msg);
                showError(msg); // Если есть ошибки, то выводим сообщение с описанием ошибки
                fptrCommon.close();
                return;
            }

            fptrCommon.setParam(Constants.LIBFPTR_PARAM_SETTING_ID, 50);                            // Устанавливаем СНО по умолчанию
            fptrCommon.setParam(Constants.LIBFPTR_PARAM_SETTING_VALUE, '4');                        // Тут выбираем СНО: 1 - ОСН, 2 - УСН (Д), 4 - УСН (Д-Р), 16 - ЕСХН, 32 - Патент
            fptrCommon.writeDeviceSetting();                                                    // Заканчиваем устанавливать

            if (fptrCommon.errorCode() != 0)                                                    // Проверяем ошибки
            {
                var msg = fptrCommon.errorDescription();
                addLog(msg);
                showError(msg);                     // Если есть ошибки, то выводим сообщение с описанием ошибки
                fptrCommon.close();
                return;
            }
        }

        /// <summary>
        /// Запустить драйвер, подключится к ФР.
        /// </summary>
        /// <returns></returns>
        private Fptr connect()
        {
            var fptrCommon = new Fptr();
            fptrCommon.setSingleSetting(Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO.ToString());            // Задаем автоматическое определение модели ККМ
            //fptrCommon .setSingleSetting(Constants.LIBFPTR_SETTING_PORT, (KKM10.LIBFPTR_PORT_COM));                    // Задаем подключение по COM-порту
            fptrCommon.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_USB.ToString());                    // Задаем подключение по USB
            //fptrCommon .setSingleSetting(Constants.LIBFPTR_SETTING_COM_FILE, "COM1");                                // Задаем номер COM-порта
            //fptrCommon .setSingleSetting(Constants.LIBFPTR_SETTING_BAUDRATE, Constants.LIBFPTR_PORT_BR_115200.ToString());        // Задаем скорость обмена

            fptrCommon.applySingleSettings();                                                                    // Применяем настройки
            fptrCommon.open();
            return fptrCommon;
        }

        /// <summary>
        /// Получить параметры ККТ.
        /// </summary>
        /// <param name="fptrCommon"></param>
        /// <returns>параметры ККТ</returns>        
        private FptrParameters getFptrParameters(Fptr fptrCommon)
        {
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
            return fptrParameters;
        }

        //Получить дату, время текущее.
        private int GetTime()
        {            
            var time = (dtpReg.Value.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (int)(time.TotalMilliseconds/1000 + 0.5);
        }

        private void addLog(string msg)
        {
            lbLog.Items.Add(msg);
        }

        private void showError(string msg)
        {
            MessageBox.Show(msg, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }



}
