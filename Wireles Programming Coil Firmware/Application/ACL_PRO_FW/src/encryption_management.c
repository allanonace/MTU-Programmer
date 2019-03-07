/***************************************************
* Module name: encryption_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/05/25 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing encryption and decryption operations
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdint.h>
#include <string.h>
#include <stdio.h>
#include "app_timer.h"
#include "aes.h"
#include "md5.h"
#include "encryption_management.h"
#include "defines_tests.h"
#include "connection_control_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/


/*  Variables section
* Add all variables here
*
***************************************************/
AES_KEY	myAESkey;

//const unsigned char myPasswd[6]={0x63,0x4C,0xC3,0xE7,0x6C,0xED}; //Password
  const unsigned char my_static_Passw[AES_BYTES]={0x54,0x68,0x69,0x73,0x20,0x69,0x73,0x20,0x74,0x68,0x65,0x20,0x50,0x61,0x73,0x73,0x77,0x6f,0x72,0x64,0x20,0x66,0x6f,0x72,0x20,0x41,0x63,0x6c,0x61,0x72,0x61,0x2e};
  unsigned char       my_dynamic_Passw[AES_BYTES];  

static void encryption_management_encrypt_pass( unsigned char *frame_in, unsigned char *frame_out,uint8_t block_size)
{
#ifdef TEST_NO_ENCRYPT
    for(uint8_t i=0;i<block_size;i++)
    { 
        frame_out[i]  = frame_in[i];
    }
    return;
#endif
    unsigned char output[64];
    AES_set_encrypt_key ((const unsigned char*)my_static_Passw,AES_BITS,&myAESkey);
    AES_encrypt((const unsigned char*)frame_in,(unsigned char*)output,(const AES_KEY*)&myAESkey);
    for(uint8_t i=0;i<block_size;i++)
    { 
        frame_out[i]  = output[i];
    }
}
  

/**************************************************
* Function name	: void encryption_management_encrypt_aes(unsigned char *frame_in, 
                            unsigned char *frame_out)
*    returns		: ---
*    arg 1			: pointer to the original frame to be encrypted
*    arg 2      : pointer to the encrypted frame
* Created by		: María Viqueira
* Date created	: 2018/05/25
* Description		: encrypts, using AES, the input frame and copies the result
*                 to output frame  
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_management_encrypt_aes(unsigned char *frame_in, unsigned char *frame_out,uint8_t block_size)
{
#ifdef TEST_NO_ENCRYPT
    for(uint8_t i=0;i<block_size;i++)
    { 
        frame_out[i]  = frame_in[i];
    }
    return;
#endif
    unsigned char output[64];
    AES_set_encrypt_key ((const unsigned char*)my_dynamic_Passw,AES_BITS,&myAESkey);
    AES_encrypt((const unsigned char*)frame_in,(unsigned char*)output,(const AES_KEY*)&myAESkey);
    for(uint8_t i=0;i<block_size;i++)
    { 
        frame_out[i]  = output[i];
    }

    
    
}

/**************************************************
* Function name	: void encryption_management_decrypt_aes(unsigned char *frame_in, 
                            unsigned char *frame_out)
*    returns		: ---
*    arg 1			: pointer to the encrypted frame
*    arg 2      : pointer to the decrypted frame
* Created by		: María Viqueira
* Date created	: 2018/05/25
* Description		: decrypts, using AES, the input frame and copies the result
*                 to output frame  
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_management_decrypt_aes(unsigned char *frame_in, unsigned char *frame_out,uint8_t block_size)
{
#ifdef TEST_NO_ENCRYPT
    for(uint8_t i=0;i<block_size;i++)
    { 
        frame_out[i]  = frame_in[i];
    }
    return;
#endif
    unsigned char output[64];
    AES_set_decrypt_key((const unsigned char*)my_dynamic_Passw,AES_BITS,&myAESkey);
    AES_decrypt((const unsigned char*)frame_in,(unsigned char*)output,(const AES_KEY*)&myAESkey);
    for(uint8_t i=0;i<block_size;i++)
    { 
        frame_out[i]  = output[i];
    }
}

/**************************************************
* Function name	: void encryption_control_management_create_new_dynamic_pass(void)
*    returns		: ---
*    arg 1			: bool true if we need new seed, otherwise, false
* Created by		: María Viqueira
* Date created	: 2018/07/17. Modified 26/07/2018
* Description		: generates a new dynamic password using MD5
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_control_management_create_new_dynamic_pass(bool new_seed)
{
    md5_state_t     state_md5;
    md5_byte_t      digest_md5[16];
    uint8_t        num_pairings_seed  = 0;
    char            seed_string[10];
    unsigned char   my_Pass_no_enc[AES_BYTES];
  
    memset(seed_string,0,sizeof(seed_string));
    num_pairings_seed  = connection_control_management_get_number_paired_times();
  
    if(new_seed)
    {
      //We are going to use as num_pairings_seed, the previous seed + 1
      if(num_pairings_seed  ==  0xFF)
      {
          num_pairings_seed = 0;
      }
      num_pairings_seed++;
    }
  
#ifdef TEST_SAME_KEY
  num_pairings  = 1;
#endif
    sprintf(seed_string,"%02X",num_pairings_seed);
    
    //MSB of dynamic Password
    md5_init(&state_md5);
    md5_append(&state_md5, (const md5_byte_t *)seed_string, strlen(seed_string));
    md5_finish(&state_md5, digest_md5);
  
    memset(my_Pass_no_enc,0,sizeof(my_Pass_no_enc));
    memset(my_dynamic_Passw,0,sizeof(my_dynamic_Passw));
  
    for(uint8_t i=0;i<LEN_MD5;i++)
    {
        my_dynamic_Passw[i] = digest_md5[i];
    }
  

    memset(seed_string,0,sizeof(seed_string));
    sprintf(seed_string,"%02X",num_pairings_seed);
    
    md5_init(&state_md5);
    md5_append(&state_md5, (const md5_byte_t *)seed_string, strlen(seed_string));
    md5_finish(&state_md5, digest_md5);
    
    memset(my_Pass_no_enc,0,sizeof(my_Pass_no_enc));
    for(uint8_t i=0;i<LEN_MD5;i++)
    {
        my_dynamic_Passw[i+LEN_MD5] = digest_md5[i];
    }
    connection_control_management_dynamic_pass_was_created();



}

/**************************************************
* Function name	: void encryption_control_management_get_dynamic_pass(unsigned char *my_pass)
*    returns		: ---
*    arg 1			: unsigned char* pointer to array to store new pass
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: Returns the dynamic password
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_control_management_get_dynamic_pass(unsigned char *my_pass )
{
    unsigned char my_dynamic_pass_encrypted[AES_BYTES];
    memset(my_dynamic_pass_encrypted,0,sizeof(my_dynamic_pass_encrypted));
  
    encryption_management_encrypt_pass(my_dynamic_Passw,my_dynamic_pass_encrypted,AES_BYTES/2);
    encryption_management_encrypt_pass(my_dynamic_Passw+16,my_dynamic_pass_encrypted+16,AES_BYTES/2);
  
    for(uint8_t i=0;i<AES_BYTES;i++)
    {
      my_pass[i] = my_dynamic_pass_encrypted[i];
    }
  
}



//#include "nrf_gpio.h"
//#include "nrf_delay.h"
//void prueba_aes(void)
//{
//  unsigned char cadena_original[20]="Hola mundo";
//  uint8_t hola_mundo_hex[20]={0x48,0x6f,0x6c,0x61,0x20,0x6d,0x75,0x6e,0x64,0x6f,0x20,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39};
//  uint8_t encrypted_char[16];
//  uint8_t passkey_encrypt[32];
//    
//  encryption_control_management_create_new_dynamic_pass();  
//  encryption_control_management_get_dynamic_pass(passkey_encrypt);
//    nrf_gpio_cfg_output(4);
//    nrf_delay_ms(500);
//    nrf_gpio_pin_set(4);
//  encryption_management_encrypt_aes(hola_mundo_hex,encrypted_char,AES_BYTES/2);  
//    nrf_gpio_pin_clear(4);

//}

