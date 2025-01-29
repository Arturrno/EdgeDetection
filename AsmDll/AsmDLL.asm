.data
r dd 0.3f 
g dd 0.59f
b dd 0.11f
; Greyscale values for each channel based on human eye sensitivity

red dq 0.3f 
green dq 0.59f
blue dq 0.11f
rounding dq 0.5        

; Registers :
    ; - RCX: pointer to the red channel
    ; - RDX: pointer to the green channel
    ; - R8:  pointer to the blue channel
    ; - R9:  pointer to the output buffer

.code
EdgeDetect proc
    ; Red
    vmovups xmm0, xmmword ptr [rcx]           ; fetch 8 bytes of data (pixels in a group)
    vpmovzxbd ymm0, xmm0                      ; extend bytes to words (16-bit)
    vcvtdq2ps ymm0, ymm0                      ; convert integer data to floating point numbers

    ; Green
    vmovups xmm1, xmmword ptr [rdx]
    vpmovzxbd ymm1, xmm1
    vcvtdq2ps ymm1, ymm1

    ; Blue
    vmovups xmm2, xmmword ptr [r8]
    vpmovzxbd ymm2, xmm2
    vcvtdq2ps ymm2, ymm2

    ; Broadcast weights to all elements of the register
    vbroadcastss ymm3, r
    vbroadcastss ymm4, g
    vbroadcastss ymm5, b                   

    ; Multiply channel data by respective weights
    vmulps ymm0, ymm0, ymm3
    vmulps ymm1, ymm1, ymm4
    vmulps ymm2, ymm2, ymm5

    ; Sum the results to get grayscale value
    vaddps ymm0, ymm0, ymm1
    vaddps ymm0, ymm0, ymm2

    ; Convert results to integer values
    vcvttps2dq ymm0, ymm0

    ; store processed data to memory // xmm0 holds 16 bytes, that is 128 bits
    pextrb byte ptr [r9+0], xmm0, 0
    pextrb byte ptr [r9+1], xmm0, 4
    pextrb byte ptr [r9+2], xmm0, 8
    pextrb byte ptr [r9+3], xmm0, 12

    ; rearrange data in ymm0 register to handle the next pixels
    vpermq ymm0, ymm0, 11111110b              ; swap lower and upper parts of the register (at the 128-bit level) cause xmm is half the ymm

    ; store processed data for the next pixels
    pextrb byte ptr [r9+4], xmm0, 0
    pextrb byte ptr [r9+5], xmm0, 4
    pextrb byte ptr [r9+6], xmm0, 8
    pextrb byte ptr [r9+7], xmm0, 12
    
    ; End of procedure
    ret
EdgeDetect endp

BlurImage proc
       ; rcx - wskanik na bufor wejsciowy 
       ; rdx - rozmiar danych (liczba pikseli w bajtach)
       ; r8 - wskaznik na bufor wyjsciowy

    xor rax, rax   

processLoop:
    cmp rax, rdx
    jge done       

    movzx r13, byte ptr [rcx + rax]     ; R
    movzx r11, byte ptr [rcx + rax + 1] ; G
    movzx r12, byte ptr [rcx + rax + 2] ; B

    ;/////////////////////////////////////

    cvtsi2sd xmm0, r13                  ; Convert R to float                
    movsd xmm1, qword ptr [red]         ; Load red channel weight
    mulsd xmm0, xmm1                    ; Multiply R by red channel weight

    addsd xmm0, qword ptr [rounding]    ; Add rounding value
    cvttsd2si r9d, xmm0                 ; Convert into integer           

    ;/////////////////////////////////////

    cvtsi2sd xmm0, r11  
    movsd xmm1, qword ptr [green]  
    mulsd xmm0, xmm1                            

    addsd xmm0, qword ptr [rounding]
    cvttsd2si r10d, xmm0           

    ;/////////////////////////////////////

    cvtsi2sd xmm0, r12              
    movsd xmm1, qword ptr [blue]   
    mulsd xmm0, xmm1                            

    addsd xmm0, qword ptr [rounding] 
    cvttsd2si r11d, xmm0           

    ;/////////////////////////////////////

    ; Add r9b, r10b, and r11b together and store the sum in r13b

    add r9b, r10b
    add r9b, r11b

    mov r13b, r9b       

    mov byte ptr [r8 + rax], r13b
    mov byte ptr [r8 + rax + 1], r13b
    mov byte ptr [r8 + rax + 2], r13b

    add rax, 3
    jmp processLoop

done:
    ret
BlurImage endp

end