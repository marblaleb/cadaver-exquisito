import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';

class ArchiveTab extends StatelessWidget {
  const ArchiveTab({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: AppColors.background,
      body: Center(child: Text('Archivo')),
    );
  }
}
