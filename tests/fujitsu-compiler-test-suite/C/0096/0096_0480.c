#include <stdio.h>
struct str {
  int  member1;
  int  member2;
};

void init(struct str *str1){
  str1->member1 = 1;
  str1->member2 = 2;
}
int main() {
  struct str str1,str2;
  init(&str1);
  str2 = str1;

  if( str2.member1 == 1 ){
    printf("OK\n");
  }else{
    printf("NG\n");
  }
}
