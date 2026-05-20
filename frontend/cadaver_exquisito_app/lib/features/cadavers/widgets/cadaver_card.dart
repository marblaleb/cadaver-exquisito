import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/available_cadavers_provider.dart';

class CadaverCard extends StatelessWidget {
  const CadaverCard({
    super.key,
    required this.cadaver,
    this.onTap,
    this.isPending = false,
  });
  final CadaverSummary cadaver;
  final VoidCallback? onTap;
  final bool isPending;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: SoftCard(
        padding: const EdgeInsets.all(20),
        child: Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    cadaver.title,
                    style: GoogleFonts.playfairDisplay(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: AppColors.textDark,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    isPending
                        ? 'Turno ${cadaver.currentTurn} de ${cadaver.maxParticipants}'
                        : 'Te toca escribir — turno ${cadaver.currentTurn}',
                    style: GoogleFonts.dmSans(
                        fontSize: 12, color: AppColors.textMuted),
                  ),
                ],
              ),
            ),
            if (!isPending)
              const Icon(Icons.arrow_forward_ios_rounded,
                  size: 14, color: AppColors.primary),
          ],
        ),
      ),
    );
  }
}
