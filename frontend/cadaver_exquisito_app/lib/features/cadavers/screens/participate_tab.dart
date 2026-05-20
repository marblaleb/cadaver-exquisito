import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/available_cadavers_provider.dart';
import '../widgets/cadaver_card.dart';
import '../widgets/create_cadaver_sheet.dart';
import '../../editor/screens/editor_screen.dart';

class ParticipateTab extends ConsumerWidget {
  const ParticipateTab({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final available = ref.watch(availableCadaversProvider);
    final pending = ref.watch(pendingCadaversProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        title: Text(
          'Cadáver Exquisito',
          style: GoogleFonts.playfairDisplay(
              color: AppColors.textDark, fontWeight: FontWeight.bold),
        ),
      ),
      floatingActionButton: FloatingActionButton(
        backgroundColor: AppColors.primary,
        onPressed: () => showModalBottomSheet(
          context: context,
          isScrollControlled: true,
          backgroundColor: Colors.transparent,
          builder: (_) => const CreateCadaverSheet(),
        ),
        child: const Icon(Icons.add, color: Colors.white),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _Section(
            title: 'TE TOCA ESCRIBIR',
            asyncValue: available,
            emptyMessage: 'No hay historias disponibles ahora.',
            onRefresh: () => ref.invalidate(availableCadaversProvider),
            itemBuilder: (cadaver) => CadaverCard(
              cadaver: cadaver,
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) =>
                        EditorScreen(cadaverId: cadaver.id)),
              ).then((_) => ref.invalidate(availableCadaversProvider)),
            ),
          ),
          const SizedBox(height: 24),
          _Section(
            title: 'EN PROGRESO',
            asyncValue: pending,
            emptyMessage: 'Ninguna historia en progreso.',
            onRefresh: () => ref.invalidate(pendingCadaversProvider),
            itemBuilder: (cadaver) =>
                CadaverCard(cadaver: cadaver, isPending: true),
          ),
        ],
      ),
    );
  }
}

class _Section extends StatelessWidget {
  const _Section({
    required this.title,
    required this.asyncValue,
    required this.emptyMessage,
    required this.itemBuilder,
    required this.onRefresh,
  });
  final String title;
  final AsyncValue<List<CadaverSummary>> asyncValue;
  final String emptyMessage;
  final Widget Function(CadaverSummary) itemBuilder;
  final VoidCallback onRefresh;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: GoogleFonts.dmSans(
              fontSize: 11,
              fontWeight: FontWeight.w600,
              color: AppColors.textMuted,
              letterSpacing: 1.2),
        ),
        const SizedBox(height: 12),
        asyncValue.when(
          data: (items) => items.isEmpty
              ? Text(emptyMessage,
                  style: GoogleFonts.dmSans(
                      color: AppColors.textMuted, fontSize: 13))
              : Column(
                  children: [
                    for (final item in items) ...[
                      itemBuilder(item),
                      const SizedBox(height: 12),
                    ]
                  ],
                ),
          loading: () =>
              const Center(child: CircularProgressIndicator()),
          error: (e, _) => Text('Error: $e'),
        ),
      ],
    );
  }
}
