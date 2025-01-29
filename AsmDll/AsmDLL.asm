.data
r dd 0.3f 
g dd 0.59f
b dd 0.11f
; Greyscale values for each channel based on human eye sensitivity

redWeight dq 0.3f 
greenWeight dq 0.59f
blueWeight dq 0.11f
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

    ; /////////////////////////////////////////////////////////////////////////////////////////

EdgeDetect2 proc
        ; rcx - wskanik na bufor wejsciowy 
        ; rdx - wskaznik na bufor wyjsciowy
        ; r8 - szerokosc
        ; r9 - wysokosc

        ; Multiplying r8 * r9 * 3 to get rough amount of color pixels in the image
        xor rax, rax   ; Zeroing rax, rax is the current pixel index
        imul r9, r8
        imul r9, 3
        mov r13, r9

        ; /////////////////////////////////////
        ; Grayscaling the image

    processLoop:
        cmp rax, r13
        jge doneGrayscale       

        movzx r10, byte ptr [rcx + rax]     ; Load R into r10
        movzx r11, byte ptr [rcx + rax + 1] ; Load G into r11
        movzx r12, byte ptr [rcx + rax + 2] ; Load B into r12 

        ; ///////////////////////////////////// R

        cvtsi2sd xmm0, r10                  ; Convert R to float                
        movsd xmm1, qword ptr [redWeight]   ; Load red channel weight
        mulsd xmm0, xmm1                    ; Multiply R by red channel weight

        addsd xmm0, qword ptr [rounding]    ; Add rounding value
        cvttsd2si r10d, xmm0                ; Convert into integer           

        ; ///////////////////////////////////// G

        cvtsi2sd xmm0, r11  
        movsd xmm1, qword ptr [greenWeight]  
        mulsd xmm0, xmm1                            

        addsd xmm0, qword ptr [rounding]
        cvttsd2si r11d, xmm0           

        ; ///////////////////////////////////// B

        cvtsi2sd xmm0, r12              
        movsd xmm1, qword ptr [blueWeight]   
        mulsd xmm0, xmm1                            

        addsd xmm0, qword ptr [rounding] 
        cvttsd2si r12d, xmm0           

        ; ///////////////////////////////////// 
        ; Summing up the values back to buffers

        ; Add r10b, r11b, and r12b together
        add r10b, r11b
        add r10b, r12b

        ; Assigning the result to the output buffer
        mov byte ptr [rdx + rax], r10b
        mov byte ptr [rdx + rax + 1], r10b
        mov byte ptr [rdx + rax + 2], r10b

        ; Assigning the result to the input buffer in order to blur image
        mov byte ptr [rcx + rax], r10b
        mov byte ptr [rcx + rax + 1], r10b
        mov byte ptr [rcx + rax + 2], r10b

        add rax, 3
        jmp processLoop

    doneGrayscale:

       

    ret
EdgeDetect2 endp

end