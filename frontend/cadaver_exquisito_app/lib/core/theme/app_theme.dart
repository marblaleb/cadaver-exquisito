import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppColors {
  static const background = Color(0xFFF0EBF4);
  static const surface = Color(0xFFE8E2EE);
  static const primary = Color(0xFFA89BB5);
  static const accent = Color(0xFFC4A882);
  static const textDark = Color(0xFF3D3347);
  static const textMuted = Color(0xFF7A7085);
  static const success = Color(0xFF8FB8A0);
  static const shadowLight = Color(0xFFFFFFFF);
  static const shadowDark = Color(0xFFC8BDD4);
}

class AppTheme {
  static ThemeData get theme => ThemeData(
        scaffoldBackgroundColor: AppColors.background,
        colorScheme: const ColorScheme.light(
          primary: AppColors.primary,
          secondary: AppColors.accent,
          surface: AppColors.surface,
          onPrimary: Colors.white,
          onSurface: AppColors.textDark,
        ),
        textTheme: GoogleFonts.dmSansTextTheme().copyWith(
          bodyLarge: GoogleFonts.dmSans(color: AppColors.textDark),
          bodyMedium: GoogleFonts.dmSans(color: AppColors.textDark),
          labelSmall: GoogleFonts.dmSans(color: AppColors.textMuted),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16)),
            elevation: 0,
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: AppColors.surface,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(16),
            borderSide: BorderSide.none,
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(16),
            borderSide:
                const BorderSide(color: AppColors.primary, width: 1.5),
          ),
        ),
      );
}

class SoftCard extends StatelessWidget {
  const SoftCard({super.key, required this.child, this.padding});
  final Widget child;
  final EdgeInsets? padding;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding ?? const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [
          BoxShadow(
            color: AppColors.shadowLight,
            offset: Offset(-4, -4),
            blurRadius: 8,
          ),
          BoxShadow(
            color: AppColors.shadowDark,
            offset: Offset(4, 4),
            blurRadius: 8,
          ),
        ],
      ),
      child: child,
    );
  }
}
