#include <stdio.h>
int main()
{     struct {
       unsigned char c: 8;
       unsigned char n: 8;
       unsigned short s: 8;
      unsigned int d: 16;
     } x = {1,2,3,5};
 
     if (x.c == 1 && x.n == 2 && x.s == 3 && x.d == 5)
         printf("ok\n");
     else
         printf("ng\n");
   }
