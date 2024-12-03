#include <string.h>
#include  <stdio.h>

int main() {
  static char x1[]={"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"};
  static char x2[]="";
#if defined(i386) || defined(__x86_64__) || defined(__aarch64__)
  static int  x3=0x00ffffff;
#elif INT64
  static int  x3=0xffffffffffffff00LL;
#else
  static int  x3=0xffffff00;
#endif
         int  s;
  printf("*** TEST START ***\n");
  s=strlen(x1);
  if (s==(sizeof(x1)-1))
       { printf(" ** NO.1 TEST OK **\n");}
  else { printf(" ## NO.1 TEST NG ##\n");}
  s=strlen(x2);
  if (s==0)
       { printf(" ** NO.2 TEST OK **\n");}
  else { printf(" ## NO.2 TEST NG ##\n");}
  s=strlen((char *)&x3);
#if INT64
  if (s==7)
#else
  if (s==3)
#endif
       { printf(" ** NO.3 TEST OK **\n"); }
  else { printf(" ## NO.3 TEST NG ## s=%d\n",s); }
  printf("***  TEST END  ***\n");
}
