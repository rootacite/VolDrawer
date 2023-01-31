
#include "mixplus.h"

#include "I2C.hpp"
#include "GPIO.hpp"
#include "Analog.hpp"
#include "Serial.hpp"

#include "usbd_cdc_if.h"

GPIO PC13(GPIOC,GPIO_PIN_13);
Serial Serial1(&huart1);
Analog Analog1(&hadc1,1);

uint8_t buffer[2];

int setVoltage(uint16_t output, int writeEEPROM) {

    uint8_t packet[3];

    if (writeEEPROM) {
        packet[0] = MCP4725_CMD_WRITEDACEEPROM;
    } else {
        packet[0] = MCP4725_CMD_WRITEDAC;
    }
    packet[1] = output / 16;        // Upper data bits (D11.D10.D9.D8.D7.D6.D5.D4)
    packet[2] = (output % 16) << 4; // Lower data bits (D3.D2.D1.D0.x.x.x.x)
    HAL_StatusTypeDef result;
    do {
        result = HAL_I2C_Master_Transmit(&hi2c1, (uint16_t) (MCP4725_I2CADDR_DEFAULT << 1), packet, 3,0xFFFF);
    }while(result!=HAL_OK);
    return 1;
}

void dataCallback(uint8_t *data,int len)
{
    uint8_t Head = data[0];
    uint16_t Vol = 0;

    switch (Head) {
        case 0:   //Write Command
            if(len<3)return;
            setVoltage(*(uint16_t*)(data+1),0);
            break;
        case 1:
            Analog1.get(&Vol);
            CDC_Transmit_FS((uint8_t *)&Vol,2);
            break;
        default:
            break;
    }
    PC13.set(0);
}

void setup()
{
    Serial1.begin();

    HAL_ADCEx_Calibration_Start(&hadc1);
}

void loop() {
   PC13.set(1);
    HAL_Delay(1);
}