
#ifndef __H_MIXPLUS
#define __H_MIXPLUS

#define MCP4725_I2CADDR_DEFAULT (96) ///< Default i2c address
#define MCP4725_CMD_WRITEDAC (0x40)    ///< Writes data to the DAC
#define MCP4725_CMD_WRITEDACEEPROM   (0x60)

#include "main.h"

#ifdef __cplusplus
extern "C" {
#endif /* __cplusplus */
    void dataCallback(uint8_t *data,int len);
	void setup();
	void loop();

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif