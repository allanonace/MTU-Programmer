#ifndef TIMER_MANAGEMENT_H
#define TIMER_MANAGEMENT_H


#define APP_TIMER_PRESCALER             0          /**< Value of the RTC1 PRESCALER register. */

void timer_management_init(void);
void timer_management_start(void);
uint32_t timer_manager_get_number_seconds(void);

#endif

