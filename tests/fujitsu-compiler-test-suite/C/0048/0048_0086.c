#include <stdio.h>

#define  print(s,t)  printf("x"#s"=%d,x"#t"=%d",x##s,x##t)
#define  x1   1
#define  x2   2
int main()
{
    print(1,2);
    printf("   ***IF IT WAS WRITTEN***   x1=1,x2=2 \n");
    printf(" ***  TEST-01 OK  LINE     ***\n\n");
}
