#ifndef BLE_DEVICE_CONFIGURATION_SERVICE_CHAR_VERSION_H__
#define BLE_DEVICE_CONFIGURATION_SERVICE_CHAR_VERSION_H__
#include <stdint.h>
#include <stdbool.h>
#include "ble.h" 
#include "ble_srv_common.h"
#include "nordic_common.h"
#include "ble_srv_common.h"
#include "app_util.h"
#include "ble_device_configuration_service.h"
#define BLE_UUID_CONFIGURATION_SERVICE_FW_CHAR                    0x0004
#define BLE_UUID_CONFIGURATION_SERVICE_HW_CHAR                    0x0008
#define LEN_BLE_DEVICE_CONFIGURATION_FW_CHAR                       4
#define LEN_BLE_DEVICE_CONFIGURATION_HW_CHAR                       4
/**************************************
* Functions
**************************************/
uint32_t ble_device_configuration_service_fw_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init);
uint32_t ble_device_configuration_service_hw_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init);
#endif // BLE_DEVICE_CONFIGURATION_SERVICE_CHAR_VERSION_H__
