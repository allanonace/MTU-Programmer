/*  Global variables 
*
*
***************************************************/
#include "ble_uart_service.h"
#include "data_management.h"
#include "ble_device_configuration_service.h"
#include "ble_uart_configuration_service.h"
#include "ble_time_configuration_service.h"
#include "ble_control_service.h"

/**************************************************
* Variables of services
**************************************************/
ble_uart_t                             m_ble_uart;  /**< Structure to identify the Virtual UART Service. */
ble_device_configuration_service_t     m_ble_device_configuration_service;
ble_uart_configuration_service_t			 m_ble_uart_configuration_service;
ble_time_configuration_service_t       m_ble_time_configuration_service;
ble_control_service_t                  m_ble_control_service;
