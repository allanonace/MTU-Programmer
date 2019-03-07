#ifndef BLE_DEVICE_CONFIGURATION_SERVICE_CHAR_BATTERY_H__
#define BLE_DEVICE_CONFIGURATION_SERVICE_CHAR_BATTERY_H__
#include <stdint.h>
#include <stdbool.h>
#include "ble.h" 
#include "ble_srv_common.h"
#include "nordic_common.h"
#include "ble_srv_common.h"
#include "app_util.h"
#include "ble_device_configuration_service.h"
#define BLE_UUID_CONFIGURATION_SERVICE_BATTERY_LEVEL_CHAR   0x000C
#define BLE_UUID_CONFIGURATION_SERVICE_BATTERY_STATUS_CHAR  0x0010
#define LEN_BLE_DEVICE_CONFIGURATION_BATTERY_LEVEL_CHAR     1
#define LEN_BLE_DEVICE_CONFIGURATION_BATTERY_STATUS_CHAR    1
/**************************************
* Functions
**************************************/
uint32_t ble_device_configuration_service_battery_level_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init);
uint32_t ble_device_configuration_service_battery_status_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init);
uint32_t ble_device_configuration_service_battery_level_update(ble_device_configuration_service_t * p_ble_device_configuration_service, uint8_t * p_string, uint16_t length);
uint32_t ble_device_configuration_service_battery_status_update(ble_device_configuration_service_t * p_ble_device_configuration_service, uint8_t * p_string, uint16_t length);

#endif // BLE_DEVICE_CONFIGURATION_SERVICE_CHAR_BATTERY_H__








