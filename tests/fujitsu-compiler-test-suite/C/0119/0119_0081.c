#include <stdlib.h>
#include <string.h>
#include <math.h>
#include  <stdio.h>
int main() {
   int    a,b,z,fun1();
   char   c[3],d[5];
   void   ok(),ng();
     a=0xa;
     z=1000;
     b=fun1(z);
     memcpy(c,"abc",3);
     memcpy(d,"12abc",5);
     if(a==b)
       {
         ok(1);
       }
     else
       {
         ng(1);
       }
     if(memcmp(c,&d[2],3)==0)
       {
         ok(2);
       }
     else
       {
         ng(2);
       }
exit (0);
}
 int fun1(a)
   int    a;
 {
     a=a/10;
     a=a-95;
     a=a*2;
     return (a);
 }
 void ok(a)
   int    a;
 {
   static char c1[]={"item1 ok"};
   static char c2[]={"item2 ok"};
     if(a==1)
       {
           printf("%s\n",c1);
       }
     else
       {
           printf("%s\n",c2);
       }
 }
 void ng(a)
   int    a;
 {
   static char c1[]={"item1 ng"};
   static char c2[]={"item2 ng"};
     if(a==1)
       {
           printf("%s\n",c1);
       }
     else
       {
           printf("%s\n",c2);
       }
 }
