/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

namespace FileFormat.Heic.Decoder
{
    internal enum CabacType
    {
        sao_merge_flag,
        sao_type_idx,
        split_cu_flag,
        cu_transquant_bypass_flag,
        cu_skip_flag,
        palette_mode_flag,
        pred_mode_flag,
        part_mode,
        prev_intra_luma_pred_flag,
        intra_chroma_pred_mode,
        rqt_root_cbf,
        merge_flag,
        merge_idx,
        inter_pred_idc,
        ref_idx,
        mvp_flag,
        split_transform_flag,
        cbf_luma,
        cbf_chroma,
        abs_mvd_greater0_flag,
        abs_mvd_greater1_flag,
        tu_residual_act_flag,
        log2_res_scale_abs_plus1,
        res_scale_sign_flag,
        transform_skip_flag,
        explicit_rdpcm_flag,
        explicit_rdpcm_dir_flag,
        last_sig_coeff_x_prefix,
        last_sig_coeff_y_prefix,
        coded_sub_block_flag,
        sig_coeff_flag,
        coeff_abs_level_greater1_flag,
        coeff_abs_level_greater2_flag,
        palette_run_prefix,
        copy_above_palette_indices_flag,
        copy_above_indices_for_final_run_flag,
        palette_transpose_flag,
        cu_qp_delta_abs,
        cu_chroma_qp_offset_flag,
        cu_chroma_qp_offset_idx
    }

}
