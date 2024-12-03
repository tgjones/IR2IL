#include <stdio.h>

#define ROTATION_NUM 100
#define ELEMENT_NUM 100
#define ANSWER 199.0f

int b_array[100];
void loop_fusion_2 (int imax, int pi) {
  int a_array[100];
  int i,bar;
  for(i=pi-3; i < imax; i++) {
    a_array[i] = 1;
  }
  for(i=pi-3; i < imax; i++) {
    b_array[i] = a_array[i] + 1;
  }
}

int main( void ) {
  double result=0;
  int i,j,k,l,m,n;
  i=j=k=l=m=n=0;

  
  printf("%s \n","initialization");
  for (i=0; i < ROTATION_NUM; ++i) {
    b_array[i] = 1;
  }

  
  i=j=3;
  printf("%s \n","Main body of processing");
  loop_fusion_2(99,i);

  
  printf("%s \n","check on result");
  for (i=0; i < ROTATION_NUM; ++i) {
    result += b_array[i];
  }

  if( (result - ANSWER) == 0 ) {
    printf("%s\n", "ok");
  } else {
    printf("%s: \n", "ng");
    printf("Correct Answer: %lf \n", ANSWER);
    printf("Execution Result: %lf \n", result);
  }
  return 1;
}
