#include <stdio.h>
int main(){
  struct s {
             signed char      b01:8;
           unsigned char      b02:8;
             signed short     b03:16;
           unsigned short     b04:16;
             signed int       b05:32;
  } y;
                       y.b02=0xff;
                       y.b03=0xffff;
  if (y.b02==0xff) 
                       printf("**(02)** OK y.b01\n");
               else    printf("**(02)** NG y.b01\n");
  if (y.b03== -1 ) 
                       printf("**(03)** OK y.b01\n");
               else    printf("**(03)** NG y.b01\n");
}
