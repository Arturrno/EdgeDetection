.data
r dd 0.3f 
g dd 0.59f
b dd 0.11f
; Greyscale values for each channel based on human eye sensitivity

.code
EdgeDetect proc

; TODO:
; change parsing 8-bit groups to parsing bigger parts of image for blur to work???
; implement working blur

; Registers :
; - RCX: pointer to the red channel
; - RDX: pointer to the green channel
; - R8:  pointer to the blue channel
; - R9:  pointer to the output buffer

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

    ; store processed data to memory
    pextrb byte ptr [r9+0], xmm0, 0          ; store value for the first pixel (red)
    pextrb byte ptr [r9+1], xmm0, 4          ; store value for the first pixel (green)
    pextrb byte ptr [r9+2], xmm0, 8          ; store value for the first pixel (blue)
    pextrb byte ptr [r9+3], xmm0, 12         ; store value for the second pixel (red)

    ; rearrange data in ymm0 register to handle the next pixels
    vpermq ymm0, ymm0, 11111110b              ; swap lower and upper parts of the register (at the 128-bit level) cause xmm is half the ymm

    ; store processed data for the next pixels
    pextrb byte ptr [r9+4], xmm0, 0          ; store value for the third pixel (red)
    pextrb byte ptr [r9+5], xmm0, 4          ; store value for the third pixel (green)
    pextrb byte ptr [r9+6], xmm0, 8          ; store value for the third pixel (blue)
    pextrb byte ptr [r9+7], xmm0, 12         ; store value for the fourth pixel (red)

    ;//////////////////////////////////////////////////////////////////////////////////////

    ; Blur the grayscale image (3x3 box blur)
        mov r10, r9                           ; Copy grayscale buffer pointer
        mov r11, r10                          ; Temporary pointer for blurring
        ;add r11, 1                            ; Start from the second pixel

    ; Loop through the image to apply blur
        mov ecx, 8                            ; Number of pixels to process
    BlurLoop:
        ; Load neighboring pixels, 8 set for tests
        movzx eax, byte ptr [r10-1]           ; Left pixel
        movzx ebx, byte ptr [r10+1]           ; Right pixel
        ;movzx edx, byte ptr [r10-1]           ; Top pixel
        ;movzx esi, byte ptr [r10+1]           ; Bottom pixel

        ; Add center pixel and its neighbors
        movzx edi, byte ptr [r10]             ; Center pixel
        add eax, ebx                          ; Left + Right
        ;add edx, esi                          ; Top + Bottom
        ;add eax, edx                          ; Left + Right + Top + Bottom
        add eax, edi                          ; Add center pixel

        ; Divide by 5 to get the average (simple box blur) ///// 3 now
        ; Use 32-bit division to avoid overflow
        mov edx, 0                            ; Clear upper 32 bits of dividend
        mov ebx, 3                            ; Divisor
        div ebx                               ; eax = (left + right + top + bottom + center) / 5

        ; Store the blurred pixel
        mov byte ptr [r11], al                ; Store the result in the blurred buffer

        ; Move to the next pixel
        inc r10
        inc r11
        loop BlurLoop

        ; End of procedure
        ret                                       ; return to the calling location

EdgeDetect endp
end